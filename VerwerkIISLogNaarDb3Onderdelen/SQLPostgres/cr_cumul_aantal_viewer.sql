CREATE TABLE IF NOT EXISTS powerbi_cumul_aantal_viewer (
  s_computername varchar(45) NOT NULL,
  datum date NOT NULL,
  uur int NOT NULL,
  s_contentpath varchar(512),
  x_forwarded_for varchar(80),
  aantal int
) ;

