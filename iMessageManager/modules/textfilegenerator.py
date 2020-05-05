import messages
import difflib

module_name = "Text File Generator"


def main():
    name = None

    while name is None:
        _input = input("Please enter a name to make a text dump of\n")
        if _input in messages.chat_name_join:
            name = _input
            continue
        close_matches = difflib.get_close_matches(_input, messages.chat_name_join, 5, 0)
        print(close_matches)

        print("Close matches found:")

        i = 1
        for match in close_matches:
            print(f"{i} | {match}")
            i += 1

        _input = input("Enter the number of the option you want, or send an invalid response to search again\n")

        if _input.isnumeric():
            if 1 <= int(_input) <= 5:
                name = close_matches[int(_input) - 1]
                continue
        print("Invalid response, please search again\n")
