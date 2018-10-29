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

namespace TokenNotifier.Controllers
{
    public class TokensController : Controller
    {
        private readonly DbCryptoContext _context;

        public TokensController(DbCryptoContext context)
        {
            _context = context;
        }

        // GET: Tokens
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tokens.ToListAsync());
        }

        // GET: Tokens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var token = await _context.Tokens
                .FirstOrDefaultAsync(m => m.TokenID == id);
            if (token == null)
            {
                return NotFound();
            }

            return View(token);
        }

        // GET: Tokens/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tokens/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TokenID,Decimals,Name,ShortName,Contract,IsObserved,PercentForNotification,TotalSupply")] Token token)
        {
            if (ModelState.IsValid)
            {
                _context.Add(token);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(token);
        }

        public IActionResult FastCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FastCreate([Bind("Contract")] Token token)
        {
            if (ModelState.IsValid)
            {
                token = TokenParser.GetTokenByContract(token.Contract);
                _context.Add(token);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(token);
        }

        // GET: Tokens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var token = await _context.Tokens.FindAsync(id);
            if (token == null)
            {
                return NotFound();
            }
            return View(token);
        }

        // POST: Tokens/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TokenID,Decimals,Name,ShortName,Contract,IsObserved,PercentForNotification,TotalSupply")] Token token)
        {
            if (id != token.TokenID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(token);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TokenExists(token.TokenID))
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
            return View(token);
        }

        // GET: Tokens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var token = await _context.Tokens
                .FirstOrDefaultAsync(m => m.TokenID == id);
            if (token == null)
            {
                return NotFound();
            }

            return View(token);
        }

        // POST: Tokens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var token = await _context.Tokens.FindAsync(id);
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TokenExists(int id)
        {
            return _context.Tokens.Any(e => e.TokenID == id);
        }
    }
}
