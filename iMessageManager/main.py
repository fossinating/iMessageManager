import os
import importlib
import messages

# Loads all modules

modules_folder = os.listdir("modules")
modules = []
for module_file in modules_folder:
    if ".py" not in module_file:
        continue
    print("modules." + module_file)
    module = importlib.import_module("modules." + module_file.replace(".py", ""))
    modules.append(module)

# Allow the user to select a module

while True:
    print("\nSelect a module to run by entering the number next to the name:\n")
    i = 1
    for module in modules:
        print(str(i) + " | " + module.name)

    finished = False
    selected_module_index = 0
    while not finished:
        finished = True
        _input = input()
        try:
            selected_module_index = int(_input)
        except TypeError:
            finished = False
            print("Please input a valid index")

    selected_module = modules[selected_module_index - 1]

    selected_module.main()
