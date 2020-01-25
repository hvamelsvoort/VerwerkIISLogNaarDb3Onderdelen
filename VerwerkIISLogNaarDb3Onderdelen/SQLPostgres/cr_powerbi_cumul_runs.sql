CREATE TABLE "powerbi_cumul_runs" (
	"s_computername" VARCHAR(45) NOT NULL,
	"datum" DATE NOT NULL,
	"uur" INTEGER NOT NULL,
	"s_contentpath" varchar(512),
	"aantal" INTEGER NULL DEFAULT NULL
)
;
