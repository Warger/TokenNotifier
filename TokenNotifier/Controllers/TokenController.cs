using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokenNotifier.Data;
using TokenNotifier.Models;

namespace TokenNotifier.Controllers
{
    public class TokenController : Controller
    {
        private readonly DbCryptoContext _context;

        public TokenController(DbCryptoContext context)
        {
            _context = context;
        }

        // GET: Token
        public ActionResult Index()
        {
            return View(_context.Tokens.ToList());
        }

        // GET: Token/Details/5
        public ActionResult Details(int id)
        {
            var token = _context.Tokens.SingleOrDefault(m => m.TokenID == id);
            if (token == null)
            {
                return NotFound();
            }

            return View(token);
        }

        // GET: Token/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Token/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TokenID")] Token tokenValue)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tokenValue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tokenValue);
        }

        // GET: Token/Edit/5
        public ActionResult Edit(int id)
        {
            var tokenValue = _context.Tokens.SingleOrDefault(m => m.TokenID == id);
            if (tokenValue == null)
            {
                return NotFound();
            }
            return View(tokenValue);
        }

        // POST: Token/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind("ID")] Token tokenValue)
        {
            if (id != tokenValue.TokenID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tokenValue);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tokens.Any(e => e.TokenID == tokenValue.TokenID))
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
            return View(tokenValue);
        }

        // GET: Token/Delete/5
        public ActionResult Delete(int id)
        {
            var tokenValue = _context.Tokens.SingleOrDefault(m => m.TokenID == id);
            if (tokenValue == null)
            {
                return NotFound();
            }

            return View(tokenValue);
        }

        // POST: Token/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tokenValue = _context.Tokens.SingleOrDefault(m => m.TokenID == id);
            _context.Tokens.Remove(tokenValue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}