/**
 * Verwerken van IIS logs naar de MySQL database.
 * Eerst de logs verzamelen als Objecten en dan in een keer schrijven.
 */
namespace VerwerkIISLogNaarDb3Onderdelen {
  class Program {
    static void Main(string[] args) {
      DeFuncties deFuncties = new DeFuncties();
      Verwerk verwerk = new Verwerk(deFuncties);

      if (verwerk.CheckArgs(args)) {
        verwerk.doen();
      } else {
        verwerk.GeefGebruik();
      }
    }
  }
}
