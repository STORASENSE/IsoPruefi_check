-- 1) Zieltabelle
CREATE TABLE IF NOT EXISTS public.location (
                                               id           BIGSERIAL PRIMARY KEY,
                                               plz          VARCHAR(10) NOT NULL,
    city         TEXT        NOT NULL,
    state        TEXT        NULL,
    country_code CHAR(2)     NOT NULL DEFAULT 'DE',
    latitude     DOUBLE PRECISION NOT NULL,
    longitude    DOUBLE PRECISION NOT NULL,
    source       TEXT        NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT now()
    );

CREATE UNIQUE INDEX IF NOT EXISTS ux_location_plz_city_country ON public.location (plz, city, country_code);
CREATE INDEX IF NOT EXISTS ix_location_plz     ON public.location (plz);
CREATE INDEX IF NOT EXISTS ix_location_city    ON public.location (city);
CREATE INDEX IF NOT EXISTS ix_location_country ON public.location (country_code);

-- 2) Staging-Tabelle passend zu GeoNames
CREATE TABLE IF NOT EXISTS public._geonames_postal (
                                                       country_code  text,
                                                       postal_code   text,
                                                       place_name    text,
                                                       admin_name1   text,
                                                       admin_code1   text,
                                                       admin_name2   text,
                                                       admin_code2   text,
                                                       admin_name3   text,
                                                       admin_code3   text,
                                                       latitude      double precision,
                                                       longitude     double precision,
                                                       accuracy      int
);

TRUNCATE public._geonames_postal;

-- 3) \copy liest die Datei vom CLIENT (dieser Init-Container), nicht vom Server.
COPY public._geonames_postal
    FROM '/seed/DE.txt'
    WITH (FORMAT text, DELIMITER E'\t', NULL '');

-- 4) Upsert in die Zieltabelle
INSERT INTO public.location (plz, city, state, country_code, latitude, longitude, source)
SELECT
    postal_code,
    place_name,
    admin_name1,
    country_code,
    latitude,
    longitude,
    'geonames'
FROM public._geonames_postal
WHERE country_code = 'DE'
    ON CONFLICT (plz, city, country_code)
DO UPDATE SET
    latitude   = EXCLUDED.latitude,
           longitude  = EXCLUDED.longitude,
           state      = COALESCE(EXCLUDED.state, public.location.state),
           updated_at = now(),
           source     = 'geonames';