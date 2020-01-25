CREATE TABLE powerbi_ingevulde_ip_adressen (
	zam_machinename TEXT NULL,
	topdesk_objectsoort VARCHAR(20) NULL DEFAULT NULL,
	special VARCHAR(10) NULL DEFAULT NULL,
	topdesk_opmerking VARCHAR(255) NULL DEFAULT NULL,
	topdesk_status VARCHAR(30) NULL DEFAULT NULL,
	topdesk_vestiging TEXT NULL,
	planon_etage VARCHAR(15) NULL DEFAULT NULL,
	topdesk_lokatie VARCHAR(20) NULL,
	image_build VARCHAR(20) NULL DEFAULT NULL,
	zam_ip_address VARCHAR(15) NOT NULL,
	topdesk_aanschafdatum VARCHAR NULL DEFAULT NULL
)
;
