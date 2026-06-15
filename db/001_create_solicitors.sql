CREATE TABLE IF NOT EXISTS locations (
    id           BIGSERIAL    PRIMARY KEY,
    name         TEXT         NOT NULL,
    last_updated TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    created_date TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_locations_name UNIQUE (name)
);

CREATE TABLE IF NOT EXISTS solicitors (
    solicitor_key TEXT        PRIMARY KEY,
    location_id   BIGINT      NOT NULL REFERENCES locations (id) ON DELETE CASCADE,
    name          TEXT        NOT NULL,
    address       TEXT,
    phone         TEXT,
    description   TEXT,
    website       TEXT,
    created_date  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_solicitors_location_id ON solicitors (location_id);
