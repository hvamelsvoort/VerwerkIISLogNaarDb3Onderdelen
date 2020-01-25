CREATE TABLE "powerbi_cumul_herkomst" (
	"s_computername" VARCHAR(45) NOT NULL,
	"datum" DATE NOT NULL,
	"uur" INTEGER NOT NULL,
	"cs_referer" varchar(2048),
	"x_forwarded_for" VARCHAR(80) NULL DEFAULT NULL,
	"aantal" INTEGER NULL DEFAULT NULL
)
;
