import messages
import difflib
import logger

module_name = "Text File Generator"


def main():
    name = None

    while name is None:
        _input = input("Please enter a name to make a text dump of\n")
        if _input in messages.chat_name_join:
            name = _input
            continue
        close_matches = difflib.get_close_matches(_input, messages.chat_name_join, 5, 0)
        logger.debug(close_matches)

        logger.my_print("Close matches found:")

        i = 1
        for match in close_matches:
            logger.my_print(f"{i} | {match}")
            i += 1

        _input = input("Enter the number of the option you want, or send an invalid response to search again\n")

        if _input.isnumeric():
            if 1 <= int(_input) <= 5:
                name = close_matches[int(_input) - 1]
                continue
        logger.debug("User gave invalid response")
        logger.my_print("Invalid response, please search again\n")

    include_non_texts = None
    while include_non_texts is None:
        _input = input("\nInclude non text-based messages? (y/n)\n")
        if _input == "y":
            include_non_texts = True
        elif _input == "n":
            include_non_texts = False
        else:
            logger.debug("User gave invalid response")
            logger.my_print("Please give a valid response")

    text = ""
    i = 0
    with open("data/text_dump.txt", "w+", encoding="utf-8") as text_dump:
        for message in messages.messages:
            if message.chat_name == name and (include_non_texts or (message.message_type == 0 and message.message_item_type == 0)):
                if message.text is None:
                    if include_non_texts:
                        text += message.alt_text
                    else:
                        continue
                else:
                    text += message.text
                i += 1
                try:
                    text_dump.write(f"{message.text} ")
                except UnicodeEncodeError:
                    fixed_string = message.text.encode("utf-8", errors="ignore")
                    logger.warning(f"Failed to write: {message.text}\nWriting: {fixed_string}")
                    text_dump.write(f"{fixed_string} ")

    logger.info(f"Added {i} messages to the text file")
