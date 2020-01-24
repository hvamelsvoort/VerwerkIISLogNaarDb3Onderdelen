using System;
using System.Collections.Generic;

namespace VerwerkIISLogNaarDb3Onderdelen {
  /// <summary>
  /// Deze class beschrijft elk IIS log bestand.
  /// Naam en Lengte. Eventueel in de toekomst unieke URLS's en uniek Query strings.
  /// </summary>
  internal class IISLogBestandObject {
    public String CompleteNaam { set; get; }
    public String Naam { set; get; }
    public long BestandsLengte { set; get; }
    public long AantalRegels { set; get; }
    public List<String> UniekeURL { set; get; }
    public List<String> UniekeQuery { set; get; }
  }
}
