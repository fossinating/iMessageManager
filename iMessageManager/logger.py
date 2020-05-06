import logging
from datetime import datetime
from logging.handlers import TimedRotatingFileHandler
from logging.handlers import RotatingFileHandler

logging.basicConfig(format="[%(levelname)s] %(asctime)s : %(message)s", datefmt="%m/%d/%Y %I:%M:%S %p",
                    level=logging.INFO)
logger = logging.getLogger()
handler = TimedRotatingFileHandler('./logs/latest.log', when="midnight", interval=1)
logger.addHandler(handler)
#logger.addHandler(logging.StreamHandler())


def critical(*messages):
    str_messages = [str(m) for m in messages]
    message = " ".join(str_messages)
    logger.critical(message)


def error(*messages):
    str_messages = [str(m) for m in messages]
    message = " ".join(str_messages)
    logger.error(message)


def warning(*messages):
    str_messages = [str(m) for m in messages]
    message = " ".join(str_messages)
    logger.warning(message)


def info(*messages):
    str_messages = [str(m) for m in messages]
    message = " ".join(str_messages)
    logger.info(message)


def debug(*messages):
    str_messages = [str(m) for m in messages]
    message = " ".join(str_messages)
    logger.debug(message)


def my_print(*messages):
    str_messages = [str(m) for m in messages]
    message = " ".join(str_messages)
    print(message)
    logger.debug(message)
