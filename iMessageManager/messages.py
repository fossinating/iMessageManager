import sqlite3

tables = {}


def import_tables():
    print("s")
    conn = sqlite3.connect("data/messages.db")
    c = conn.cursor()
    c.execute("SELECT name FROM sqlite_master WHERE type='table';")
    for table in c.fetchall():
        try:
            c.execute(f"SELECT * from {table[0]};")
            result = list(c.fetchall())
            print(table[0] + " : " + str(len(result)) + " entries")
            tables[table[0]] = result
        except sqlite3.OperationalError:
            pass
    #print(tables)
    print("done")


def import_contacts():
    contacts =


if len(tables) == 0:
    import_tables()
