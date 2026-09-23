import os
from datetime import date, datetime, time
from pathlib import Path

import psycopg
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
from fastapi.staticfiles import StaticFiles

BASE_DIR = Path(__file__).resolve().parent
load_dotenv(BASE_DIR / ".env")

app = FastAPI(title="Lab 3")
app.mount("/static", StaticFiles(directory=BASE_DIR / "static"), name="static")

QUERIES = {
    1: "SELECT id, name, phone, pay FROM public.table_bastrakov_lab2",
    2: "SELECT id, name, address FROM public.table_bastrakov_lab2 ORDER BY address ASC",
    3: "SELECT * FROM table_bastrakov_lab2 WHERE enrolled_date < NOW() - INTERVAL '4 years'",
}


def _conn():
    return psycopg.connect(
        host=os.getenv("PG_HOST", "localhost"),
        port=int(os.getenv("PG_PORT", "5432")),
        dbname=os.getenv("PG_DB", "lab2"),
        user=os.getenv("PG_USER", "postgres"),
        password=os.getenv("PG_PASSWORD", ""),
    )


def _serialize(value):
    if isinstance(value, (datetime, date, time)):
        return value.isoformat()
    return value


def run_query(sql: str) -> dict:
    try:
        with _conn() as conn, conn.cursor() as cur:
            cur.execute(sql)
            columns = [desc.name for desc in cur.description or ()]
            rows = [[_serialize(v) for v in row] for row in cur.fetchall()]
        return {"columns": columns, "rows": rows}
    except psycopg.Error as exc:
        raise HTTPException(status_code=500, detail=f"Database error: {exc}") from exc


@app.get("/")
def index():
    return FileResponse(BASE_DIR / "static" / "index.html")


@app.get("/api/table")
def api_table():
    return run_query("SELECT * FROM public.table_bastrakov_lab2 ORDER BY id ASC")


@app.get("/api/query/{query_id:int}")
def api_query(query_id: int):
    sql = QUERIES.get(query_id)
    if sql is None:
        raise HTTPException(status_code=404, detail=f"Query {query_id} not found")
    return {"query": query_id, **run_query(sql)}