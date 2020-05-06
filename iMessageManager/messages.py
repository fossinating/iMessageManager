import sqlite3
import csv
from datetime import datetime
import logger

tables = {}
chat_id_join = []
chat_name_join = []
chat_service_join = []
chat_message_join = []
messages = []
contacts = {}


class Message(object):
    message_id = -1
    text = ""
    service = ""
    date = 0
    from_me = None
    message_type = 0
    message_item_type = 0
    chat_id = 0
    chat_name = ""

    def __init__(self, message_id: int, text: str, alt_text: str, service: str, date: int, from_me: bool, message_type: int, message_item_type: int):
        self.message_id = message_id
        self.text = text
        self.alt_text = alt_text
        self.service = service
        self.date = date
        self.from_me = from_me
        self.message_type = message_type
        self.message_item_type = message_item_type
        self.chat_id = chat_message_join[message_id - 1]
        self.chat_name = chat_name_join[self.chat_id - 1]


def import_tables():
    conn = sqlite3.connect("data/messages.db")
    c = conn.cursor()
    c.execute("SELECT name FROM sqlite_master WHERE type='table';")
    for table in c.fetchall():
        try:
            c.execute(f"SELECT * from {table[0]};")
            result = list(c.fetchall())
            logger.debug(table[0] + " : " + str(len(result)) + " entries")
            tables[table[0]] = result
        except sqlite3.OperationalError:
            pass
    logger.debug(tables.keys())
    logger.debug("done")


def import_contacts():
    with open("data/contacts.csv", mode="r", encoding="utf8", errors="ignore") as contacts_file:
        contacts_file.readline()
        for l in csv.reader(contacts_file, quotechar='"', delimiter=",", quoting=csv.QUOTE_ALL, skipinitialspace=True):
            name = l[0]
            raw_number = l[34]
            number_parts = [str(s) for s in raw_number if s.isdigit()]
            if len(number_parts) == 0:
                continue
            if len(number_parts) == 11:
                number_parts.pop(0)
            number = "".join(number_parts)
            logger.debug(name, raw_number, number_parts, number)
            contacts[number] = name


def import_chats():
    chat_list = tables["chat"]
    for chat in chat_list:
        chat_id_join.append("")
        chat_name_join.append("")
        chat_service_join.append("")
    for chat in chat_list:
        chat_id = chat[0]
        chat_identifier = chat[6]
        service_name = chat[7]
        display_name = chat[12]
        chat_id_join[chat_id - 1] = chat_identifier
        if display_name != "":
            name = chat[12]
        elif chat_identifier.startswith("chat"):
            name = f"Unnamed {service_name} group chat"
        elif chat_identifier.startswith("+1"):
            if chat_identifier.strip("+1") in contacts:
                name = contacts[chat_identifier.strip("+1")]
                logger.debug(f"found {chat_identifier.strip('+1')} in contacts as {name}")
            else:
                name = chat_identifier.strip("+1")
        else:
            if chat_identifier in contacts:
                name = contacts[chat_identifier]
            else:
                name = chat_identifier
        chat_name_join[chat_id - 1] = name
        chat_service_join[chat_id - 1] = service_name

    chat_message_join_table = tables["chat_message_join"]
    max_id = 0
    for message in chat_message_join_table:
        max_id = max(max_id, message[1])
    for i in range(0, max_id):
        chat_message_join.append(None)
    for message in chat_message_join_table:
        logger.debug(f"Joining {message[1] - 1}/{len(chat_message_join)} ({round((message[1] - 1)/(len(chat_message_join))*100)}%)")
        chat_message_join[message[1] - 1] = message[0]


def import_messages():
    messages_table = tables["message"]

    for message in messages_table:
        message_id = message[0]
        text = message[2]
        alt_text = ""
        service = message[11]
        date = message[15]
        from_me = message[21]
        item_type = message[41]
        message_type = message[52]
        if text is None:
            balloon_bundle_id = message[53]
            handle_id = message[5]
            other_handle = message[42]
            if balloon_bundle_id is not None:
                alt_text = balloon_bundle_id
                logger.debug(f"{message_id} has null text, replaced with balloon_bundle_id")
            elif item_type == 1:
                alt_text = f"{handle_id} added {other_handle} to the conversation"
                logger.debug(f"{message_id} has null text (added someone to conversation)")
            elif item_type == 2:
                alt_text = f'{handle_id} named the conversation "{message[43]}"'
                logger.debug(f"{message_id} has null text (renamed conversation)")
            elif item_type == 3:
                alt_text = f"{handle_id} left the conversation"
            elif item_type == 4:
                alt_text = f"{handle_id} started location sharing with {other_handle}"
                logger.debug(f"{message_id} has null text (sharing location)")
            elif item_type == 6:
                alt_text = f"{handle_id} started a FaceTime in {chat_name_join[chat_message_join[message_id - 1]]}"
                logger.debug(f"{message_id} has null text (started group facetime)")
            else:
                logger.warning(f"{message_id} has null text and couldn't be fixed")
        if chat_message_join[message_id - 1] is None:
            logger.info(f"{message_id} skipped as chat is null")
            continue
        messages.append(Message(message_id, text, alt_text, service, date, from_me, message_type, item_type))


if len(tables) == 0:
    import_tables()
    import_contacts()
    import_chats()
    import_messages()
    logger.info("Done initializing")
