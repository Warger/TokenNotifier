using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TokenNotifier.Data;
using TokenNotifier.Models;
using TokenNotifier.Parser;
using TokenNotifier.ViewModels;

namespace TokenNotifier.Controllers
{
    public class ExchangesController : Controller
    {
        private readonly DbCryptoContext _context;

        public ExchangesController(DbCryptoContext context)
        {
            _context = context;
        }

        // GET: Exchanges
        public async Task<IActionResult> Index()
        {
            List<ExchangeForView> exchangeToView = new List<ExchangeForView>();
            foreach (Exchange e in _context.Exchange)
            {
                ExchangeForView etv = new ExchangeForView();
                etv.Exchange = e;
                
                etv.Tokens = new List<ExchangeToken>();
                List<ExchangeToken> exTokens = new List<ExchangeToken>();
                _context.WatchedTokens.Where(wt => wt.Exchange.ID == e.ID && wt.Counter > 1
                ).ToList().ForEach(wt => etv.Tokens.Add(new ExchangeToken() { AddDateTime = wt.DateTime, TokenName = wt.Name }));
                exchangeToView.Add(etv);
            }

            return View(exchangeToView);
            //  return View(await _context.Exchange.ToListAsync());
        }

        // GET: Exchanges/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exchange = await _context.Exchange
                .FirstOrDefaultAsync(m => m.ID == id);
            if (exchange == null)
            {
                return NotFound();
            }

            ExchangeForView exchangeToView = new ExchangeForView();
            exchangeToView.Exchange = exchange;
            exchangeToView.Tokens = new List<ExchangeToken>();
            List<ExchangeToken> exTokens = new List<ExchangeToken>();
            _context.WatchedTokens.Where(wt => wt.Exchange.ID == id && wt.Counter > 1
            ).ToList().ForEach(wt => exchangeToView.Tokens.Add(new ExchangeToken() { AddDateTime = wt.DateTime, TokenName = wt.Name }));
            exchangeToView.Tokens = exchangeToView.Tokens.OrderByDescending(k => k.AddDateTime).ToList();

            return View(exchangeToView);
        }

        // GET: Exchanges/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Exchanges/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title")] Exchange exchange)
        {
            if (ModelState.IsValid)
            {
                exchange.NotifyOnNextUpdate = false;
                _context.Add(exchange);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(exchange);
        }

        // GET: Exchanges/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exchange = await _context.Exchange.FindAsync(id);
            if (exchange == null)
            {
                return NotFound();
            }
            return View(exchange);
        }

        public async Task<IActionResult> UpdateExchange(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exchange = await _context.Exchange
                .FirstOrDefaultAsync(m => m.ID == id);

            if (exchange == null)
            {
                return NotFound();
            }

            try
            {
                ExchangeUpdater.RemoveAllWatchedTokens(_context, (int)id);
                ExchangeUpdater.Update(_context);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExchangeExists(exchange.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Details), new { id = exchange.ID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateExchange(int id, [Bind("ID,Title")] Exchange exchange)
        {
            if (id != exchange.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ExchangeUpdater.Update(_context);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExchangeExists(exchange.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Exchanges/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title")] Exchange exchange)
        {
            if (id != exchange.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exchange);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExchangeExists(exchange.ID))
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
            return View(exchange);
        }

        // GET: Exchanges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exchange = await _context.Exchange
                .FirstOrDefaultAsync(m => m.ID == id);
            if (exchange == null)
            {
                return NotFound();
            }

            return View(exchange);
        }

        // POST: Exchanges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exchange = await _context.Exchange.FindAsync(id);
            _context.Exchange.Remove(exchange);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExchangeExists(int id)
        {
            return _context.Exchange.Any(e => e.ID == id);
        }
    }
}
