namespace VerwerkIISLogNaarDb3Onderdelen {

  //          0    1    2            3             4         5              6           7        8        9    10       11        12             
  //#Fields:  date time cs-uri-query s-contentpath sc-status s-computername cs(Referer) sc-bytes cs-bytes c-ip cs(Host) cs-method cs(User-Agent) 
  //          13   14              15           16     17         18         19         20          21  
  //          s-ip X-Forwarded-For sc-substatus s-port cs-version c-protocol cs(Cookie) TimeTakenMS cs-uri-stem
  internal class LogVeldIndex {
    public static int datum = 0;
    public static int tijd = 1;
    public static int cs_uri_query = 2;
    public static int s_contentpath = 3;
    public static int sc_status = 4;
    public static int s_computername = 5;
    public static int cs_referer = 6;
    public static int sc_bytes = 7;
    public static int cs_bytes = 8;
    public static int c_ip = 9;
    public static int cs_host = 10;
    public static int cs_method = 11;
    public static int cs_user_agent = 12;
    public static int s_ip = 13;
    public static int x_forwarded_for = 14;
    public static int sc_substatus = 15;
    public static int s_port = 16;
    public static int cs_version = 17;
    public static int c_protocol = 18;
    public static int cs_cookie = 19;
    public static int time_taken = 20;
    public static int cs_uri_stem = 21;

  }
}