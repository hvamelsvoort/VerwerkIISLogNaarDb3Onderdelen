using System;

namespace VerwerkIISLogNaarDb3Onderdelen {
  internal class TijdsduurObject {
    internal string s_computername;
    internal DateTime datum;
    internal string tijd;
    internal string cs_method;
    internal string sc_status;
    internal string s_contentpath;
    internal string time_taken = "0";

    public override bool Equals(object obj) {
      TijdsduurObject tijdsduurObject = obj as TijdsduurObject;

      return tijdsduurObject != null
        && tijdsduurObject.s_computername == this.s_computername
        && tijdsduurObject.datum == this.datum
        && tijdsduurObject.tijd == this.tijd
        && tijdsduurObject.s_computername == this.s_computername
        && tijdsduurObject.cs_method == this.cs_method
        && tijdsduurObject.sc_status == this.sc_status
        && tijdsduurObject.s_contentpath == this.s_contentpath
        ;
    }

    public override int GetHashCode() {
      return (this.s_computername == null ? 0 : this.s_computername.GetHashCode())
       ^ (this.datum == null ? 0 : this.datum.GetHashCode())
       ^ (this.tijd == null ? 0 : this.tijd.GetHashCode())
       ^ (this.cs_method == null ? 0 : this.cs_method.GetHashCode())
       ^ (this.sc_status == null ? 0 : this.sc_status.GetHashCode())
       ^ (this.s_contentpath == null ? 0 : this.s_contentpath.GetHashCode());

    }
  }
}