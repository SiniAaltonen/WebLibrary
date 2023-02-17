using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kirjasto.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Kirjasto.Controllers
{
    public class KäyttäjäController : Controller
    {
        private readonly KirjastoDBContext _context;
        private readonly IConfiguration _conf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KäyttäjäController(KirjastoDBContext context, IConfiguration conf, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _conf = conf;
            _webHostEnvironment = webHostEnvironment;
        }

        // KÄYTTÄJIEN HALLINTAAN LIITTYVÄT TOIMINNOT

        // Lista käyttäjistä
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListaaKäyttäjät()
        {
            var kirjastoDBContext = _context.Käyttäjäs;
            return View(await kirjastoDBContext.ToListAsync());
        }

        // Asiakkaan kirjautumissivu
        public async Task<IActionResult> Index()
        {
              return View();
        }

        // Kirjastonhoitajan kirjautumissivu (luotu yhdet admin-tunnukset)
        public async Task<IActionResult> Admin()
        {
            return View("Admin");
        }

        //Käyttäjän tietojen muokkaaminen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KäyttäjäId,Etunimi,Sukunimi,Käyttäjänimi,Salasana,OnkoAdmin")] Käyttäjä käyttäjä)
        {
            if (id != käyttäjä.KäyttäjäId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(käyttäjä);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KäyttäjäExists(käyttäjä.KäyttäjäId))
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
            return View(käyttäjä);
        }

        // Käyttäjän poistaminen
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Käyttäjäs == null)
            {
                return NotFound();
            }

            var käyttäjä = await _context.Käyttäjäs
                .FirstOrDefaultAsync(m => m.KäyttäjäId == id);
            if (käyttäjä == null)
            {
                return NotFound();
            }

            return View(käyttäjä);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Käyttäjäs == null)
            {
                return Problem("Entity set 'KirjastoDBContext.Käyttäjäs'  is null.");
            }
            var käyttäjä = await _context.Käyttäjäs.FindAsync(id);
            if (käyttäjä != null)
            {
                _context.Käyttäjäs.Remove(käyttäjä);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KäyttäjäExists(int id)
        {
          return (_context.Käyttäjäs?.Any(e => e.KäyttäjäId == id)).GetValueOrDefault();
        }

        // Käyttäjän omat tiedot käyttäjäid:llä
        [HttpGet("Käyttäjä/{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Käyttäjäs == null)
            {
                return NotFound();
            }

            var käyttäjä = await _context.Käyttäjäs
                .FirstOrDefaultAsync(m => m.KäyttäjäId == id);
            if (käyttäjä == null)
            {
                return NotFound();
            }

            if (käyttäjä.VarausSaapunut == true)
            {
                ViewBag.Viesti = "Varaamasi nimeke on nyt saatavilla. Siirry varauksiisi.";
            }
            return View(käyttäjä);
        }

        // Käyttäjän omat tiedot käyttäjänimellä
        public async Task<IActionResult> Details(string käyttäjänimi)
        {
            if (käyttäjänimi == null || _context.Käyttäjäs == null)
            {
                return NotFound();
            }

            var käyttäjä = await _context.Käyttäjäs
                .FirstOrDefaultAsync(m => m.Käyttäjänimi == käyttäjänimi);
            
            if (käyttäjä == null)
            {
                return NotFound();
            }

            if (käyttäjä.VarausSaapunut == true)
            {
                ViewBag.Teksti = "Varaamasi kirja on lainattavissa. Tarkista varauslista.";
            }

            return View(käyttäjä);
        }


        // Luo uusi käyttäjätunnus
        public IActionResult Create()
        {
            return View();
        }

        // Käyttäjien muokkaaminen id:n perusteella
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Käyttäjäs == null)
            {
                return NotFound();
            }

            var käyttäjä = await _context.Käyttäjäs.FindAsync(id);
            if (käyttäjä == null)
            {
                return NotFound();
            }
            return View(käyttäjä);
        }

        // Luodaan tavalliset käyttäjätunnukset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KäyttäjäId,Etunimi,Sukunimi,Käyttäjänimi,Salasana,OnkoAdmin")] Käyttäjä käyttäjä)
        {
            if (_context.Käyttäjäs.Any(k => k.Käyttäjänimi == käyttäjä.Käyttäjänimi)) // Varmistetaan, että käyttäjänimet on yksilöllisiä
            {
                ViewBag.Virhe = "Käyttäjätunnus on jo olemassa. Valitse toinen tunnus.";
                return View();
            }

            if (ModelState.IsValid)
            {
                _context.Add(käyttäjä);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(käyttäjä);
        }

        // Luodaan admin-tunnukset (kirjastonhoitaja)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin([Bind("KäyttäjäId,Etunimi,Sukunimi,Käyttäjänimi,Salasana,OnkoAdmin")] Käyttäjä käyttäjä)
        {
            // Tähän ei nyt enää tehdä varmistusta, koska admin-tunnuksia on vain yksi
            if (ModelState.IsValid)
            {
                käyttäjä.OnkoAdmin = true;
                _context.Add(käyttäjä);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(käyttäjä);
        }

        // KÄYTTÄJIEN OMINAISUUKSIIN LIITTYVÄT TOIMINNOT (omille sivuille)
        public async Task<IActionResult> ListaLainauksista(int? id)
        {
            ViewBag.id = id;
            var lainaukset = _context.Lainaus.Include(ki => ki.Kirja).Include(k => k.Käyttäjä).Where(l => l.KäyttäjäId == id);
            return View(await lainaukset.ToListAsync());
        }

        public async Task<IActionResult> ListaVarauksista(int? id)
        {
            ViewBag.id = id;
            var varaukset = _context.Varaus.Include(ki => ki.Kirja).Where(v => v.KäyttäjäId == id);
            ViewBag.käyttäjäid = id;
            return View(await varaukset.ToListAsync());
        }



        // TÄSTÄ ALKAA KIRJAUTUMIS- JA AUTENTIKOINTIOSIO, EI SAA POISTAA

        [HttpPost]
        [Route("/Index")]
        public async Task<IActionResult> KirjauduAsiakkaana(string käyttäjänimi, string salasana)
        {
            var loginValid = ValidateLogin(käyttäjänimi, salasana);

            if (!loginValid)
            {
                TempData["LoginFailed"] = $"Virheellinen käyttäjänimi tai salasana.";

                return Redirect("/Create"); // Tähän pitää tehdä joku järkevä error-näkymä
            }
            else
            {
                await KirjaaAsiakas(käyttäjänimi);
                var käyttäjä = _context.Käyttäjäs.Where(k => k.Käyttäjänimi == käyttäjänimi).FirstOrDefault();
                int id = käyttäjä.KäyttäjäId;

                return RedirectToAction("Details", new { id = id});
            }
        }
       

        [HttpPost]
        public async Task<IActionResult> KirjauduAdmin(string käyttäjänimi, string salasana)
        {
            var loginValid = ValidateAdminLogin(käyttäjänimi, salasana);

            if (!loginValid)
            {
                TempData["LoginFailed"] = $"Virheellinen käyttäjänimi tai salasana.";

                return Redirect("/Create");
            }
            else
            {
                await KirjaaAdmin(käyttäjänimi);

                return Redirect("/Kirja/Index"); // Tähän linkki admin-sivuille, kun ne on valmiit. Eivät muuten valmistuneet.
            }
        }

        private bool ValidateLogin(string käyttäjänimi, string salasana)
        {

            var käyttäjä = _context.Käyttäjäs.Where(k => k.Käyttäjänimi == käyttäjänimi).FirstOrDefault();

            if (käyttäjä.Salasana == salasana)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Admin-kirjautumisen varmistaminen (vain alisa-tunnuksilla)
        private bool ValidateAdminLogin(string käyttäjänimi, string salasana)
        {
            var käyttäjä = _context.Käyttäjäs.Where(k => k.Käyttäjänimi == käyttäjänimi).FirstOrDefault();

            if (käyttäjä.Salasana == salasana && (käyttäjä.Käyttäjänimi == "alisa"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        // Luodaan user-identity asiakkaalle
        private async Task KirjaaAsiakas(string käyttäjänimi)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, käyttäjänimi),
                new Claim(ClaimTypes.Role, "Asiakas"),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        // Admin-kirjautumista ei enää tarvita, ei laiteta linkkiä verkkosivulle
        private async Task KirjaaAdmin(string käyttäjänimi)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, käyttäjänimi),
                new Claim(ClaimTypes.Role, "Admin"),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        // Sovelluksesta uloskirjautuminen
        public async Task<IActionResult> KirjauduUlos()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Home/Index");
        }

    }
}
