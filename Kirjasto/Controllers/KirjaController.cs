using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kirjasto.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace Kirjasto.Controllers
{
    public class KirjaController : Controller
    {
        private readonly KirjastoDBContext _context;
        private readonly IConfiguration _conf;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public KirjaController(KirjastoDBContext context, IConfiguration conf, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _conf = conf;
            _webHostEnvironment = webHostEnvironment;
        }

        // Kirjalista haku aakkosilla
        public async Task<IActionResult> Index(string alkukirjain = "k")
        {
            var aakkoset = new List<string>();
            foreach (var k in _context.Kirjas)
            {
                aakkoset.Add(k.Nimi[0].ToString());
            }

            ViewBag.aakkoset = aakkoset.Distinct().OrderBy(k => k);

            var kirjastoDBContext = _context.Kirjas.Include(k => k.Kirjailija);
            return View(await kirjastoDBContext.Where(k => k.Nimi.StartsWith(alkukirjain)).ToListAsync());
        }

        // Kirjalista haku avainsanoilla

        public async Task<IActionResult> KirjatAvainsanoilla(string avainsana = "C#")
        {
            var avainsanatPilkuilla = "";

            foreach (var item in _context.Kirjas)
            {
                avainsanatPilkuilla += $"{item.Avainsanat},";
            }

            var avainsanat = avainsanatPilkuilla.Split(",");

            ViewBag.avainsanat = avainsanat.Distinct();

            var kirjastoDBContext = _context.Kirjas.Include(k => k.Kirjailija);
            return View(await kirjastoDBContext.Where(k => k.Avainsanat.Contains(avainsana)).ToListAsync());
        }

        // Kirjan tiedot
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Kirjas == null)
            {
                return NotFound();
            }

            var kirja = await _context.Kirjas
                .Include(k => k.Kirjailija)
                .FirstOrDefaultAsync(m => m.KirjaId == id);
            if (kirja == null)
            {
                return NotFound();
            }

            if (kirja.Lainassa == true || kirja.Varaus.Count > 1)
            {
                ViewBag.Teksti = "Kirja on tällä hetkellä lainassa. Voit tehdä varauksen lainauslinkistä.";
            }

            if (kirja.Lainassa == false && kirja.Varaus.Count < 1)
            {
                ViewBag.Teksti = "Kirja on lainattavissa.";
            }

            return View(kirja);
        }

        public IActionResult KehotusKirjautua()
        {
            return View();
        }

        [Authorize(Roles = "Asiakas")] // Ainoastaan kirjautuneeet asiakkaat voivat lainata
        public async Task<IActionResult> Lainaa(int id)
        {
            // Etsitään kirja id:n perusteella
            var kirja = await _context.Kirjas
                .Include(k => k.Kirjailija)
                .Include(l => l.Lainaus)
                .Include(v => v.Varaus) // Tarvitaanko nämä kaikki? En osaa sanoa.
                .FirstOrDefaultAsync(m => m.KirjaId == id);

            // Otetaan kirjautuneen käyttäjän käyttäjänimi
            var käyttäjänimi = User.Identity.Name;

            // Etsitään tietokannasta oikea käyttäjä User Identityn perusteella
            var käyttäjä = _context.Käyttäjäs.Where(k => k.Käyttäjänimi == käyttäjänimi).SingleOrDefault();
            var käyttäjäID = käyttäjä.KäyttäjäId;

            // Tarkistetaan, onko kirja jo lainassa
            if (kirja.Lainassa == true)
            {
                return RedirectToAction("Varaus", new { id = id, käyttäjäID = käyttäjäID });
            }

            // Tarkistetaan, onko kirjaan varauksia
            if (kirja.Varaus.Any())
            {
                var varaukset = kirja.Varaus.ToArray();
                var ekaVaraus = varaukset[0];
                var varaajaid = varaukset[0].KäyttäjäId;
                if (varaajaid != käyttäjäID) // Ainoastaan ensimmäinen varauslistassa voi tehdä lainauksen
                {
                    return RedirectToAction("Varaus", new { id = id, käyttäjäID = käyttäjäID });
                }
                // Jos ensimmäinen varaaja on tekemässä lainausta, käyttäjän varauksen ja ilmoituksen voi poistaa
                var varaaja = _context.Käyttäjäs.Find(varaajaid);
                varaaja.VarausSaapunut = false;
                varaaja.Varaus.Remove(ekaVaraus);
            }

            //Luodaan uusi Lainaus-olio

            DateTime lainauspvm = DateTime.Now;
            DateTime eräpvm = lainauspvm.AddDays(14);
            Lainau lainaus = new Lainau() { LainausPvm = lainauspvm, EräPvm = eräpvm, KirjaId = id };

            //Lisätään käyttäjän lainauksiin ja tietokantaan
            käyttäjä.Lainaus.Add(lainaus);
            kirja.Lainassa = true;
            _context.Lainaus.Add(lainaus); // Tarvitaanko tätä? En tiedä.
            _context.SaveChanges();

            ViewBag.Teksti = $"Lainattu käyttäjälle '{käyttäjä.Käyttäjänimi}'. Palauta {eräpvm} mennessä.";

            return View(kirja);
        }

        // Kirjan palauttaminen, eli lainausolio poistetaan
        public async Task<IActionResult> Palauta(int? id)
        {

            if (id == null || _context.Lainaus == null)
            {
                return ViewBag.Virhe = "Tapahtui virhe. Palauttaminen ei onnistunut.";
            }

            var lainaus = await _context.Lainaus
                .FirstOrDefaultAsync(l => l.LainausId == id);
            var käyttäjäid = lainaus.KäyttäjäId;
            var kirjaid = lainaus.KirjaId;
            var kirja = _context.Kirjas.Find(kirjaid);

            //Tarkistetaan, onko kirjasta varauksia
            if (kirja.Varaus.Any())
            {
                var varaaja = (from k in _context.Käyttäjäs
                               join v in _context.Varaus on k.KäyttäjäId equals v.KäyttäjäId
                               join ki in _context.Kirjas on v.KirjaId equals ki.KirjaId
                               select k).FirstOrDefault();

                varaaja.VarausSaapunut = true; // Käyttäjän omissa tiedoissa viesti, että varaus on saapunut
                _context.SaveChanges();
            }

            if (lainaus == null)
            {
                return ViewBag.Virhe = "Tapahtui virhe. Palauttaminen ei onnistunut.";
            }

            _context.Lainaus.Remove(lainaus);
            //kirja.Lainassa = false;

            await _context.SaveChangesAsync();

            return RedirectToAction("ListaLainauksista", "Käyttäjä", new { id = käyttäjäid });
        }

        public IActionResult Varaus(int id, int käyttäjäid)
        {
            var kirja = _context.Kirjas.Find(id);
            var käyttäjä = _context.Käyttäjäs.Find(käyttäjäid);

            if (_context.Varaus.Any(k => k.KirjaId == id && k.KäyttäjäId == käyttäjäid))
            {
                ViewBag.Id = käyttäjäid;
                ViewBag.Teksti = $"Olet jo varannut nimekkeen '{kirja.Nimi}'. Saat ilmoituksen, kun kirja on saatavilla.";
                return View();
            }

            else
            {
                // Luodaan uusi Varaus-olio ja lisätään tietokantaan
                Varau varaus = new Varau() { KirjaId = id, KäyttäjäId = käyttäjäid };
                _context.Varaus.Add(varaus);
                kirja.Varaus.Add(varaus);
                _context.SaveChanges();

                ViewBag.Id = käyttäjäid;
                ViewBag.Teksti = $"Valitettavasti nimeke '{kirja.Nimi}' on jo lainattu, teimme varauksen käyttäjänimelle {käyttäjä.Käyttäjänimi}. Saat ilmoituksen omilla sivuilla, kun kirja on saatavilla";
                return View();
            }
        }

        public async Task<IActionResult> PoistaVaraus(int? id)
        {

            if (id == null || _context.Varaus == null)
            {
                return ViewBag.Virhe = "Tapahtui virhe. Palauttaminen ei onnistunut.";
            }

            var varaus = await _context.Varaus
                .FirstOrDefaultAsync(v => v.VarausId == id);
            var käyttäjäid = varaus.KäyttäjäId;
            var kirjaid = varaus.KirjaId;
            var kirja = _context.Kirjas.Find(kirjaid);

            // Poistetaan varaus
            _context.Varaus.Remove(varaus);
            _context.SaveChanges();

            // Tarkistetaan, onko kirjasta muita varauksia

            if (kirja.Varaus.Any())
            {
                var varaaja = (from k in _context.Käyttäjäs
                               join v in _context.Varaus on k.KäyttäjäId equals v.KäyttäjäId
                               join ki in _context.Kirjas on v.KirjaId equals ki.KirjaId
                               select k).FirstOrDefault();

                varaaja.VarausSaapunut = true; // Käyttäjän omissa tiedoissa viesti, että varaus on saapunut
                _context.SaveChanges();
            }

            if (varaus == null)
            {
                return ViewBag.Virhe = "Tapahtui virhe. Varaaminen ei onnistunut.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ListaVarauksista", "Käyttäjä", new { id = käyttäjäid });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["KirjailijaId"] = new SelectList(_context.Kirjailijas, "KirjailijaId", "KirjailijaId");
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KirjaId,Nimi,Kuvaus,Tyyppi,Avainsanat,NäytäKuvaus,Lainassa,KirjailijaId")] Kirja kirja)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kirja);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KirjailijaId"] = new SelectList(_context.Kirjailijas, "KirjailijaId", "KirjailijaId", kirja.KirjailijaId);
            return View(kirja);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Kirjas == null)
            {
                return NotFound();
            }

            var kirja = await _context.Kirjas.FindAsync(id);
            if (kirja == null)
            {
                return NotFound();
            }
            ViewData["KirjailijaId"] = new SelectList(_context.Kirjailijas, "KirjailijaId", "KirjailijaId", kirja.KirjailijaId);
            return View(kirja);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KirjaId,Nimi,Kuvaus,Tyyppi,Avainsanat,NäytäKuvaus,Lainassa,KirjailijaId")] Kirja kirja)
        {
            if (id != kirja.KirjaId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kirja);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KirjaExists(kirja.KirjaId))
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
            ViewData["KirjailijaId"] = new SelectList(_context.Kirjailijas, "KirjailijaId", "KirjailijaId", kirja.KirjailijaId);
            return View(kirja);
        }

        // Kirjan poistaminen
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Kirjas == null)
            {
                return NotFound();
            }

            var kirja = await _context.Kirjas
                .Include(k => k.Kirjailija)
                .FirstOrDefaultAsync(m => m.KirjaId == id);
            if (kirja == null)
            {
                return NotFound();
            }

            return View(kirja);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Kirjas == null)
            {
                return Problem("Entity set 'KirjastoDBContext.Kirjas'  is null.");
            }
            var kirja = await _context.Kirjas.FindAsync(id);
            if (kirja != null)
            {
                _context.Kirjas.Remove(kirja);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KirjaExists(int id)
        {
          return (_context.Kirjas?.Any(e => e.KirjaId == id)).GetValueOrDefault();
        }
    }
}
