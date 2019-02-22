using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using TokenNotifier.Data;
using TokenNotifier.Models;
using TokenNotifier.ViewModels;
using System.Data.Entity.Infrastructure;
using TokenNotifier.Parser;

namespace TokenNotifier.Controllers
{
    public class WalletsController : Controller
    {
        private readonly DbCryptoContext _context;

        public WalletsController(DbCryptoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {           
            var qry = _context.Wallets.AsNoTracking().Include(x => x.Exchange).OrderBy(x=> x.WalletID);
            var model = await PagingList.CreateAsync(qry, 50, page);
            return View(model);
        }

        // GET: Wallets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(m => m.WalletID == id);
            if (wallet == null)
            {
                return NotFound();
            }

            List<Transfer> transfers = new List<Transfer>();
            List<Transfer> dbTransfers = _context.Transfers.Where(t => t.IncomingAddress == wallet.Address || t.OutgoingAddress == wallet.Address).OrderBy(t => t.Date).ToList();
            transfers.AddRange(dbTransfers);

            List<TransferForView> tfw = new List<TransferForView>();

            foreach (Transfer t in transfers)
            {
                Wallet inW, outW;
                inW = _context.Wallets.FirstOrDefault(w => w.Address == t.IncomingAddress);
                outW = _context.Wallets.FirstOrDefault(w => w.Address == t.OutgoingAddress);

                Token tok = _context.Tokens.FirstOrDefault(token => token.Contract == t.Token);

                double supPercent = tok == null ? 0 : (t.Value / tok.TotalSupply);

                tfw.Add(new TransferForView(t, inW == null ? new Wallet() { Name = "", Address = t.IncomingAddress } : inW,
                    outW == null ? new Wallet() { Name = "", Address = t.OutgoingAddress } : outW, tok == null ? t.Token : tok.ShortName,
                    supPercent.ToString("N4") + " %"));
            }

            WalletForView wfv = new WalletForView(wallet, tfw);

            return View(wfv);
        }

        // GET: Wallets/Create
        public IActionResult Create()
        {
            ExchangesDropDownList();
            return View();
        }

        // POST: Wallets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WalletID,Name,Address,Comment,ExchangeId")] Wallet wallet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wallet);
                if (wallet.Exchange != null)
                {
                    wallet.Exchange.NotifyOnNextUpdate = false;
                }

                wallet.Exchange = GetExhangeById(wallet.ExchangeId == null ? 0 : (int)wallet.ExchangeId);
                if (wallet.Exchange == null)
                    wallet.ExchangeId = null;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ExchangesDropDownList(wallet.Exchange);
            return View(wallet);
        }

        // GET: Wallets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wallet = await _context.Wallets.FindAsync(id);
            if (wallet == null)
            {
                return NotFound();
            }
            ExchangesDropDownList(wallet.Exchange);

            return View(wallet);
        }

        // POST: Wallets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WalletID,Name,Address,Comment,ExchangeId")] Wallet wallet)
        {
            if (id != wallet.WalletID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wallet);

                    wallet.Exchange = GetExhangeById(wallet.ExchangeId == null ? 0 : (int)wallet.ExchangeId);

                    if (wallet.Exchange != null)
                    {
                        wallet.Exchange.NotifyOnNextUpdate = false;
                    }
                    else
                    {
                        wallet.ExchangeId = null;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException)
                {
                    if (!WalletExists(wallet.WalletID))
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
            ExchangesDropDownList(wallet.Exchange);

            return View(wallet);
        }

        // GET: Wallets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(m => m.WalletID == id);
            if (wallet == null)
            {
                return NotFound();
            }

            return View(wallet);
        }

        // POST: Wallets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wallet = await _context.Wallets.FindAsync(id);
            _context.Wallets.Remove(wallet);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WalletExists(int id)
        {
            return _context.Wallets.Any(e => e.WalletID == id);
        }

        private void ExchangesDropDownList(object selectedExchanges = null)
        {
            var exhangesQuery = from d in _context.Exchange
                                   orderby d.Title
                                   select d;
            ViewBag.ExchangeID = new SelectList(exhangesQuery, "ID", "Title", selectedExchanges);
        }

        private Exchange GetExhangeById(int id)
        {
            return _context.Exchange.FirstOrDefault(e => e.ID == id);
        }
    }
}
