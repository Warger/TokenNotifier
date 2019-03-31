using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using TokenNotifier.Data;
using TokenNotifier.Models;
using TokenNotifier.Parser;
using TokenNotifier.ViewModels;

namespace TokenNotifier.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly DbCryptoContext _context;

        public NotificationsController(DbCryptoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, string filter = "All", int page = 1)
        {
       //     TwitterParser.Update(_context);

            List<SelectListItem> filterItems = new List<SelectListItem>();
            filterItems.Add(new SelectListItem() { Text = "All", Value = "", Selected = true });

            _context.Tokens.OrderBy(x => x.Name).ToList().ForEach(t =>
             {
                 filterItems.Add(new SelectListItem() { Text = t.Name, Value = t.Name });
             });
            ViewBag.Filter = filterItems.Select(x => x.Text);
            ViewBag.ChosenFilter = filter;
            ViewBag.ChosenPage = page;

            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "Date" : "";
            ViewBag.PriceSortParm = sortOrder == "Price" ? "price_desc" : "Price";

            List<Notification> notificationsForView;

            // Filter
            if (filter == "All")
                notificationsForView = _context.Notifications.AsNoTracking().Include(x => x.LinkedWallet).ToList();
            else
                notificationsForView = _context.Notifications.AsNoTracking().Where(n => n.Description.Contains("[" + filter)).Include(x => x.LinkedWallet).ToList();

            // Sorting
            List<NotificationForView> nfvl = new List<NotificationForView>();
            List<Notification> notifications;
            switch (sortOrder)
            {
                case "price_desc":
                    notifications = notificationsForView.OrderByDescending(x => x.USDValue).ToList();
                    break;
                case "Price":
                    notifications = notificationsForView.OrderBy(x => x.USDValue).ToList();
                    break;
                case "Date":
                    notifications = notificationsForView.OrderBy(x => x.DateTime).ToList();
                    break;
                default:
                    notifications = notificationsForView.OrderByDescending(x => x.DateTime).ToList();
                    break;
            }

            foreach (Notification n in notifications)
                nfvl.Add(new NotificationForView(n, n.LinkedWallet.Address, n.LinkedWallet.NameOrAddress));

            var res = nfvl.AsEnumerable();
            var model = PagingList.Create(res, 50, page);

            model.RouteValue = new RouteValueDictionary {
                { "filter", filter},
                { "sortOrder", sortOrder }
            };

            return View(model);
        }
        

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.NotificationID == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NotificationID,Description,Action,DateTime,Value")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NotificationID,Description,Action,DateTime,Value")] Notification notification)
        {
            if (id != notification.NotificationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.NotificationID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.NotificationID == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.NotificationID == id);
        }
    }
}
