CREATE TABLE "powerbi_cumul_gebruiker" (
	"s_computername" VARCHAR(45) NOT NULL,
	"datum" DATE NOT NULL,
	"uur" INTEGER NOT NULL,
	"x_forwarded_for" VARCHAR(80) NULL DEFAULT NULL,
	"aantal" INTEGER NULL DEFAULT NULL
)
;