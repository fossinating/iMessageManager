import os
import importlib
import logger
from kivy.app import App
from kivy.lang import Builder
from kivy.uix.button import Button
from kivy.uix.gridlayout import GridLayout
from kivy.uix.screenmanager import Screen, ScreenManager

# Loads all modules
'''
modules_folder = os.listdir("modules")
modules = []
for module_file in modules_folder:
    if ".py" not in module_file:
        continue
    logger.debug("modules." + module_file)
    module = importlib.import_module("modules." + module_file.replace(".py", ""))
    modules.append(module)
'''


class MainMenu(Screen):
    pass


class StatsMenu(Screen):
    pass


class GraphsMenu(Screen):
    pass


class SearchMenu(Screen):
    pass


class UpdateDataMenu(Screen):
    pass


class StartMenu(Screen):
    pass


class MenuManager(ScreenManager):
    pass


kv = Builder.load_file("layout.kv")


class MainApp(App):
    def build(self):
        return kv


MainApp().run()

'''
while True:
    print("\nSelect a module to run by entering the number next to the name:\n")
    i = 1
    for module in modules:
        print(str(i) + " | " + module.module_name)

    finished = False
    selected_module_index = 0
    while not finished:
        finished = True
        _input = input()
        try:
            selected_module_index = int(_input)
            selected_module = modules[selected_module_index - 1]
        except ValueError:
            finished = False
            print("Please input a valid index")
        except IndexError:
            finished = False
            print("Please input a valid index within the range")

    selected_module = modules[selected_module_index - 1]

    selected_module.main()
'''