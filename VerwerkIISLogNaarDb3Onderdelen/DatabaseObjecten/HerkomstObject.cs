using System;

namespace VerwerkIISLogNaarDb3Onderdelen {
  internal class HerkomstObject {
    internal string s_computername;
    internal DateTime datum;
    internal string uur;
    internal string cs_referer;
    internal string x_forwarded_for;

    public override bool Equals(object obj) {
      HerkomstObject herkomst = obj as HerkomstObject;

      return herkomst != null
        && herkomst.s_computername == this.s_computername
        && herkomst.datum == this.datum
        && herkomst.uur == this.uur
        && herkomst.cs_referer == this.cs_referer
        && herkomst.x_forwarded_for == this.x_forwarded_for
        ;
    }

    public override int GetHashCode() {
      return (this.s_computername == null ? 0 : this.s_computername.GetHashCode())
       ^ (this.datum == null ? 0 : this.datum.GetHashCode())
       ^ (this.uur == null ? 0 : this.uur.GetHashCode())
       ^ (this.cs_referer == null ? 0 : this.cs_referer.GetHashCode())
       ^ (this.x_forwarded_for == null ? 0 : this.x_forwarded_for.GetHashCode());

    }
  }
}