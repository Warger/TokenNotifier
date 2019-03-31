using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TokenNotifier.Data;
using TokenNotifier.Models;

namespace TokenNotifier.Controllers
{
    public class TwitterAccountsController : Controller
    {
        private readonly DbCryptoContext _context;

        public TwitterAccountsController(DbCryptoContext context)
        {
            _context = context;
        }

        // GET: TwitterAccounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.TwitterAccount.ToListAsync());
        }

        // GET: TwitterAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var twitterAccount = await _context.TwitterAccount
                .FirstOrDefaultAsync(m => m.TwitterAccountId == id);
            if (twitterAccount == null)
            {
                return NotFound();
            }

            return View(twitterAccount);
        }

        // GET: TwitterAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TwitterAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TwitterAccountId,Name,LastUpdated,Template")] TwitterAccount twitterAccount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(twitterAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(twitterAccount);
        }

        // GET: TwitterAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var twitterAccount = await _context.TwitterAccount.FindAsync(id);
            if (twitterAccount == null)
            {
                return NotFound();
            }
            return View(twitterAccount);
        }

        // POST: TwitterAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TwitterAccountId,Name,LastUpdated,Template")] TwitterAccount twitterAccount)
        {
            if (id != twitterAccount.TwitterAccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(twitterAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TwitterAccountExists(twitterAccount.TwitterAccountId))
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
            return View(twitterAccount);
        }

        // GET: TwitterAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var twitterAccount = await _context.TwitterAccount
                .FirstOrDefaultAsync(m => m.TwitterAccountId == id);
            if (twitterAccount == null)
            {
                return NotFound();
            }

            return View(twitterAccount);
        }

        // POST: TwitterAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var twitterAccount = await _context.TwitterAccount.FindAsync(id);
            _context.TwitterAccount.Remove(twitterAccount);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TwitterAccountExists(int id)
        {
            return _context.TwitterAccount.Any(e => e.TwitterAccountId == id);
        }
    }
}
