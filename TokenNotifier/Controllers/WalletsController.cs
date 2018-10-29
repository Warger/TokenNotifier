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
            var qry = _context.Wallets.AsNoTracking().OrderBy(x=> x.WalletID);
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
                    supPercent.ToString("0.000000") + " %"));
            }

            WalletForView wfv = new WalletForView(wallet, tfw);

            return View(wfv);
        }

        // GET: Wallets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Wallets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WalletID,Name,Address")] Wallet wallet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wallet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
            return View(wallet);
        }

        // POST: Wallets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WalletID,Name,Address")] Wallet wallet)
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
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
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
    }
}
