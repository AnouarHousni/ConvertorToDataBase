using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertorToDataBase.Enums
{
    public enum NpgsqlDataType
    {
        SMALLINT,
        INTEGER,
        BIGINT,
        NUMERIC,
        REAL,
        DOUBLE_PRECISION,
        DATE,
        TIME,
        TIMESTAMP,
        TIMESTAMPTZ,
        CHAR,
        VARCHAR,
        TEXT,
        JSON,
        JSONB,
        UUID,
        XML,
        BYTEA,
        ARRAY,
        INET,
        CIDR,
        MACADDR,
        GEOMETRY,
        GEOGRAPHY
    }
}
