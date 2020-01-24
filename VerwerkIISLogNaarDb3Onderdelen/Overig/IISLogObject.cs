using System;

namespace VerwerkIISLogNaarDb3Onderdelen {
  /// <summary>
  /// Weergave van de Log velden 
  // Sinds maart / april 2018
  //# Start-Date: 2018-03-30 09:20:15.306
  //         0    1    2            3             4         5              6           7        8        9    10       11        12             13   14              15           16     17         18         19         20          21
  //#Fields: date time cs-uri-query s-contentpath sc-status s-computername cs(Referer) sc-bytes cs-bytes c-ip cs(Host) cs-method cs(User-Agent) s-ip X-Forwarded-For sc-substatus s-port cs-version c-protocol cs(Cookie) TimeTakenMS cs-uri-stem
  /// 
  /// </summary>
  public class IISLogObject {
    public DateTime datum { get; set; }
    public string uur { get; set; }
    public string fractie_tijd { get; set; }
    public string cs_uri_query { get; set; }
    public string s_contentpath { get; set; }
    public string sc_status { get; set; }
    public string s_computername { get; set; }
    public string cs_referer { get; set; }
    public string sc_bytes { get; set; }
    public string cs_bytes { get; set; }
    public string c_ip { get; set; }
    public string cs_host { get; set; }
    public string cs_method { get; set; }
    public string cs_user_agent { get; set; }
    public string s_ip { get; set; }
    public string x_forwarded_for { get; set; }
    public string sc_substatus { get; set; }
    public string s_port { get; set; }
    public string cs_version { get; set; }
    public string c_protocol { get; set; }
    public string cs_cookie { get; set; }
    public string time_taken { get; set; }

    /**
     * Construct nieuw object
     */
    public IISLogObject() {
      cs_uri_query = "nvt";
      s_contentpath = "nvt";
      sc_status = "0";
      cs_referer = "nvt";
      sc_bytes = "0";
      cs_bytes = "0";
      c_ip = "000.000.000.000";
      cs_host = "nvt";
      cs_method = "nvt";
      cs_user_agent = "nvt";
      s_ip = "000.000.000.000";
      x_forwarded_for = "000.000.000.000";
      sc_substatus = "";
      s_port = "0";
      cs_version = "nvt";
      c_protocol = "nvt";
      cs_cookie = "nvt";

      time_taken = "0";
      s_computername = "nvt";

    }
    //public override bool Equals(object obj) {
    //  IISLogObject iSLogObject = obj as IISLogObject;

    //  return iSLogObject != null
    //    && iSLogObject.s_computername == this.s_computername
    //    && iSLogObject.datum == this.datum
    //    && iSLogObject.uur == this.uur
    //    && iSLogObject.fractie_tijd == this.fractie_tijd
    //    && iSLogObject.cs_uri_query == this.cs_uri_query
    //    && iSLogObject.s_contentpath == this.s_contentpath
    //    ;

    //}

    //public override int GetHashCode() {
    //  return (this.s_computername == null ? 0 : this.s_computername.GetHashCode())
    //   ^ (this.datum == null ? 0 : this.datum.GetHashCode())
    //   ^ (this.uur == null ? 0 : this.uur.GetHashCode())
    //   ^ (this.fractie_tijd == null ? 0 : this.fractie_tijd.GetHashCode())
    //   ^ (this.cs_uri_query == null ? 0 : this.cs_uri_query.GetHashCode())
    //   ^ (this.s_contentpath == null ? 0 : this.s_contentpath.GetHashCode());

    //}
  }
}
