using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReflectionIT.Mvc.Paging;
using TokenNotifier.Data;
using TokenNotifier.Models;
using TokenNotifier.Parser;
using TokenNotifier.ViewModels;

namespace TokenNotifier.Controllers
{
    public class TransfersController : Controller
    {
        private readonly DbCryptoContext _context;
        private Updater u;
        private readonly ILogger _logger;

        public TransfersController(DbCryptoContext context, Updater u)
        {
            _context = context;
            this.u = u;
            //     _logger = logger.CreateLogger("TransferController");

        }

        // GET: Transfers
     /*   public async Task<IActionResult> Index()
        {
            return View(await _context.Transfers.OrderByDescending(x => x.Date).ToListAsync());
        }
        */

        public async Task<IActionResult> Index(int page = 1)
        {
            var model = PagingList.Create(_context.Transfers.OrderByDescending(x => x.Date), 50, page);

            List<TransferForView> nfvl = new List<TransferForView>();
            foreach (Transfer t in model)
            {
                Wallet inW, outW;
                inW = _context.Wallets.FirstOrDefault(w => w.Address == t.IncomingAddress);
                outW = _context.Wallets.FirstOrDefault(w => w.Address == t.OutgoingAddress);

                Token tok = _context.Tokens.FirstOrDefault(token => token.Contract == t.Token);

                double supPercent = tok == null ? 0 : (t.Value / tok.TotalSupply);

                nfvl.Add(new TransferForView(t, inW == null ? new Wallet() { Name="", Address=t.IncomingAddress } : inW, 
                    outW == null ? new Wallet() { Name = "", Address = t.OutgoingAddress } : outW, tok == null ? t.Token : tok.ShortName,
                    supPercent.ToString("0.000000") + " %" ));
            }
            nfvl.AddRange(Enumerable.Repeat(new TransferForView(), (_context.Transfers.Count()-50*page)).ToList());
            List<TransferForView> newList = Enumerable.Repeat(new TransferForView(), (50 * (page - 1))).ToList();
            newList.AddRange(nfvl);

            return View(PagingList.Create(newList, 50, page));
        }

        // GET: Transfers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transfer = await _context.Transfers
                .FirstOrDefaultAsync(m => m.TrasferID == id);
            if (transfer == null)
            {
                return NotFound();
            }

            return View(transfer);
        }

        // GET: Transfers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Transfers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrasferID,IncomingAddress,OutgoingAddress,Date,Value,Token,UsdValue")] Transfer transfer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transfer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(transfer);
        }

        // GET: Transfers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transfer = await _context.Transfers.FindAsync(id);
            if (transfer == null)
            {
                return NotFound();
            }
            return View(transfer);
        }

    /*    // GET: Transfers/Update
        public async Task<IActionResult> Update()
        {
            //            _logger.LogError("Update was call");
            ILog log = LogManager.GetLogger(typeof(Program));
            log.Info("Update was call");
            await u.Update(_context);
            return View(await _context.Transfers.ToListAsync());
        }
        */

        // POST: Transfers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TrasferID,IncomingAddress,OutgoingAddress,Date,Value,Token,UsdValue")] Transfer transfer)
        {
            if (id != transfer.TrasferID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transfer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransferExists(transfer.TrasferID))
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
            return View(transfer);
        }

        // GET: Transfers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transfer = await _context.Transfers
                .FirstOrDefaultAsync(m => m.TrasferID == id);
            if (transfer == null)
            {
                return NotFound();
            }

            return View(transfer);
        }

        // POST: Transfers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transfer = await _context.Transfers.FindAsync(id);
            _context.Transfers.Remove(transfer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransferExists(int id)
        {
            return _context.Transfers.Any(e => e.TrasferID == id);
        }
    }
}
