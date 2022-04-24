def FAN_ON():
    global FAN_status
    FAN_status = 1
    serial.write_string("!3:FAN:" + str(FAN_status) + "#")
def FAN_OFF():
    global FAN_status
    FAN_status = 0
    serial.write_string("!4:FAN:" + str(FAN_status) + "#")
def init_timer_sofware():
    global TIMER_CYCLE, timerCounter, timerFlag
    TIMER_CYCLE = 10
    timerCounter = [0, 0]
    timerFlag = [0, 0]
    set_timer(0, TIMER_CYCLE)
    set_timer(1, TIMER_CYCLE)

def on_button_pressed_a():
    Toggle_LED()
input.on_button_pressed(Button.A, on_button_pressed_a)

def Toggle_FAN():
    if FAN_status == 0:
        FAN_ON()
    else:
        FAN_OFF()
def set_timer(index: number, value: number):
    if index >= 0 or index < len(timerCounter):
        timerCounter[index] = value / TIMER_CYCLE
        timerFlag[index] = 0
def LED_OFF():
    global LED_status
    LED_status = 0
    serial.write_string("!1:LED:" + str(LED_status) + "#")
def LED_ON():
    global LED_status
    LED_status = 1
    serial.write_string("!1:LED:" + str(LED_status) + "#")

def on_button_pressed_b():
    Toggle_FAN()
input.on_button_pressed(Button.B, on_button_pressed_b)

def on_data_received():
    global cmd
    cmd = serial.read_until(serial.delimiters(Delimiters.DOLLAR))
    basic.show_string(cmd)
    if cmd == "#LED_ON":
        FAN_ON()
        basic.show_leds("""
            . . # . .
                        . # # . .
                        . . # . .
                        . . # . .
                        . # # # .
        """)
    elif cmd == "#LED_OFF":
        LED_OFF()
        basic.show_leds("""
            . # # # .
                        . . . # .
                        . # # # .
                        . # . . .
                        . # # # .
        """)
    elif cmd == "#FAN_ON":
        FAN_ON()
        basic.show_leds("""
            . # # # .
                        . . . # .
                        . # # # .
                        . . . # .
                        . # # # .
        """)
    elif cmd == "#FAN_OFF":
        FAN_OFF()
        basic.show_leds("""
            . # . # .
                        . # . # .
                        . # # # .
                        . . . # .
                        . . . # .
        """)
    else:
        pass
serial.on_data_received(serial.delimiters(Delimiters.HASH), on_data_received)

def Toggle_LED():
    if LED_status == 0:
        LED_ON()
    else:
        LED_OFF()
def timer_run():
    i_timer = 0
    while i_timer <= len(timerCounter):
        if timerCounter[i_timer] > 0:
            timerCounter[i_timer] = timerCounter[i_timer] - 1
            if timerCounter[i_timer] <= 0:
                timerFlag[i_timer] = 1
        i_timer += 1
def reset_timer(num: number):
    if num >= 0 or num < len(timerCounter):
        timerCounter[num] = 0
        timerFlag[num] = 0
cmd = ""
LED_status = 0
timerFlag: List[number] = []
timerCounter: List[number] = []
TIMER_CYCLE = 0
FAN_status = 0
init_timer_sofware()
basic.show_icon(IconNames.DIAMOND)

def on_forever():
    if timerFlag[0] == 1:
        serial.write_string("!1:TEMP:" + str(input.temperature()) + "#")
        set_timer(0, 5000)
    if timerFlag[1] == 1:
        serial.write_string("!2:LIGHT:" + str(input.light_level()) + "#")
        set_timer(1, 5000)
    timer_run()
    basic.pause(10)
basic.forever(on_forever)
