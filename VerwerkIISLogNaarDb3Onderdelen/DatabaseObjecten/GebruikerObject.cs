using System;

namespace VerwerkIISLogNaarDb3Onderdelen {
  internal class GebruikerObject {
    internal string s_computername;
    internal DateTime datum;
    internal string uur;
    internal string x_forwarded_for;
    public override bool Equals(object obj) {

      GebruikerObject gebruikerObject = obj as GebruikerObject;

      return gebruikerObject != null
        && gebruikerObject.s_computername == this.s_computername
        && gebruikerObject.datum == this.datum
        && gebruikerObject.uur == this.uur
        && gebruikerObject.x_forwarded_for == this.x_forwarded_for
        ;
    }

    public override int GetHashCode() {
      return (this.s_computername == null ? 0 : this.s_computername.GetHashCode())
       ^ (this.datum == null ? 0 : this.datum.GetHashCode())
       ^ (this.uur == null ? 0 : this.uur.GetHashCode())
       ^ (this.x_forwarded_for == null ? 0 : this.x_forwarded_for.GetHashCode());

    }
  }
}