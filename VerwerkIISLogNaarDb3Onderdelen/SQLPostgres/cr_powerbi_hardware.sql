-- Table: public.powerbi_hardware

DROP TABLE public.powerbi_hardware;

CREATE TABLE public.powerbi_hardware
(
    zam_machinename text COLLATE pg_catalog."default",
    topdesk_objectsoort character varying(20) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    special character varying(10) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    topdesk_opmerking character varying(255) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    topdesk_status character varying(20) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    topdesk_gearchiveerd character varying(25) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    topdesk_vestigiging text COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    planon_etage character varying(20) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    topdesk_lokatie character varying(20) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    zam_systemproduct character varying(35) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    image_build character varying(15) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    image_version character varying(20) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    patchlevel character varying(15) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    patchgroep character varying(15) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    topdesk_objecttype character varying(25) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    laatstgespoeld character varying(30) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    zam_ip_address character varying(15) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    zam_last_scandate character varying(20) COLLATE pg_catalog."default" DEFAULT NULL::character varying,
    topdesk_aanschafdatum text COLLATE pg_catalog."default",
    zam_lanaddress character VARYING(15) COLLATE pg_catalog."default" DEFAULT NULL::character VARYING
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.powerbi_hardware
    OWNER to postgres;

-- Index: indexIp

-- DROP INDEX public."indexIp";

CREATE UNIQUE INDEX "indexIp"
    ON public.powerbi_hardware USING btree
    (zam_ip_address COLLATE pg_catalog."default")
    TABLESPACE pg_default;
    