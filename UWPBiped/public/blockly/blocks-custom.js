'use strict';

goog.provide('Blockly.Blocks.device');

goog.require('Blockly.Blocks');

var blockColors = {
    basic: 190,
    led: 300,
    input: 3,
    loops: 120,
    pins: 351,
    music: 52,
    game: 176,
    comments: 156,
    images: 45,
    speech: 165,
}

var ledStateDropdown = [
  ["on", "1"],
  ["off", "0"]
];

var ledColorDropdown = [
  ["red", "Windows.UI.Colors.red"],
  ["yellow", "Windows.UI.Colors.yellow"],
  ["blue", "Windows.UI.Colors.blue"],
  ["green", "Windows.UI.Colors.green"],
  ["black", "Windows.UI.Colors.black"],
  ["white", "Windows.UI.Colors.white"]
];

var accelAxisDropdown = [
  ["x", "0"],
  ["y", "1"],
  ["z", "2"]
];

var analogChannelDropdown = [
  ["0", "0"],
  ["1", "1"],
  ["2", "2"],
  ["3", "3"],
  ["4", "4"],
  ["5", "5"],
  ["6", "6"],
  ["7", "7"]
];

var joystickButtonDropdown = [
  ["fire", "IoTBlocklyHelper.SenseHatJoystickButton.enter"],
  ["up", "IoTBlocklyHelper.SenseHatJoystickButton.up"],
  ["down", "IoTBlocklyHelper.SenseHatJoystickButton.down"],
  ["left", "IoTBlocklyHelper.SenseHatJoystickButton.left"],
  ["right", "IoTBlocklyHelper.SenseHatJoystickButton.right"],
  ["any", "IoTBlocklyHelper.SenseHatJoystickButton.any"]
];

Blockly.Blocks['device_pause'] = {
  init: function() {
    this.setHelpUrl('https://www.microbit.co.uk/functions/pause');
    this.setColour(blockColors.basic);
    this.appendDummyInput()
        .appendField("pause (ms)");
    this.appendValueInput("PAUSE")
        .setCheck("Number");
    this.setInputsInline(true);
    this.setPreviousStatement(true);
    this.setNextStatement(true);
    this.setTooltip('Stop execution for the given delay, hence allowing other threads of execution to run.');
  }
};

Blockly.Blocks['device_forever'] = {
  init: function() {
    this.setHelpUrl('https://www.microbit.co.uk/functions/forever');
    this.setColour(blockColors.basic);
    this.appendDummyInput()
        .appendField("forever");
    this.appendStatementInput("DO")
        .setCheck("null");
    this.setInputsInline(true);
    this.setPreviousStatement(true);
    this.setTooltip('Run a sequence of operations repeatedly, in the background.');
  }
};


Blockly.Blocks['device_get_acceleration'] = {
    init: function () {
        this.setHelpUrl('https://www.microbit.co.uk/functions/acceleration');
        this.setColour(blockColors.input);
        this.appendDummyInput()
            .appendField("acceleration (mg)");
        this.appendDummyInput()
            .appendField(new Blockly.FieldDropdown(accelAxisDropdown), "AXIS");
        this.setInputsInline(true);
        this.setOutput(true, "Number");
        this.setTooltip('Get the acceleration on an axis (between -2048 and 2047).');
    }
};

Blockly.Blocks['device_get_compass'] = {
    init: function () {
        this.setColour(blockColors.input);
        this.appendDummyInput()
            .appendField("compass heading (°)");
        this.setInputsInline(true);
        this.setOutput(true, "Number");
        this.setTooltip('Find which direction on a compass the device is facing.');
    }
};

Blockly.Blocks['device_get_temperature'] = {
    init: function () {
        this.setColour(blockColors.input);
        this.appendDummyInput()
            .appendField("temperature (°C)");
        this.setInputsInline(true);
        this.setOutput(true, "Number");
        this.setTooltip('Returns the temperature measured in Celsius (metric).');
    }
};

Blockly.Blocks['device_get_humidity'] = {
    init: function () {
        this.setColour(blockColors.input);
        this.appendDummyInput()
            .appendField("humidity");
        this.setInputsInline(true);
        this.setOutput(true, "Number");
        this.setTooltip('Find the humidity level where you are. Humidity is measured in % (0 - 100 % rH).');
    }
};

Blockly.Blocks['device_get_pressure'] = {
    init: function () {
        this.setColour(blockColors.input);
        this.appendDummyInput()
            .appendField("barometric pressure");
        this.setInputsInline(true);
        this.setOutput(true, "Number");
        this.setTooltip('Find the barometric pressure level where you are. Absolute pressure is measured in hPA (260 to 1260 hPa). 1 hPa is 1 millibar.');
    }
};

Blockly.Blocks['device_get_running_time'] = {
    init: function () {
        this.setColour(blockColors.input);
        this.appendDummyInput()
            .appendField("running time (ms)");
        this.setInputsInline(true);
        this.setOutput(true, "Number");
        this.setTooltip('Find how long it has been since the program started.');
    }
};

Blockly.Blocks['device_set_onboard_led'] = {
    init: function () {
        // TODO (alecont): link below is not right
        this.setHelpUrl('https://www.microbit.co.uk/functions/digital-write-pin');
        this.setColour(blockColors.basic);
        this.appendDummyInput()
            .appendField("turn onboard LED ")
            .appendField(new Blockly.FieldDropdown(ledStateDropdown), "STATE");
        this.setInputsInline(true);
        this.setPreviousStatement(true);
        this.setNextStatement(true);
        this.setTooltip('Turn the onboard LED on or off (works only on Raspberry Pi 2, not on Raspberry Pi 3).');
    }
};

Blockly.Blocks['device_digital_write_pin'] = {
    init: function () {
        // TODO (alecont): link below is not right
        this.setHelpUrl('https://www.microbit.co.uk/functions/digital-write-pin');
        this.setColour(blockColors.pins);
        this.appendDummyInput()
            .appendField("digital write ");
        this.appendValueInput("VALUE")
            .setCheck("Number");
        this.appendDummyInput()
            .appendField(" to pin ");
        this.appendValueInput("PIN")
            .setCheck("Number");
        this.setInputsInline(true);
        this.setPreviousStatement(true);
        this.setNextStatement(true);
        this.setTooltip('Turn a specific a GPIO pin on or off.');
    }
};

Blockly.Blocks['device_analog_read_channel'] = {
    init: function () {
        this.setColour(blockColors.pins);
        this.appendDummyInput()
            .appendField("analog read pin");
        this.appendDummyInput()
            .appendField(new Blockly.FieldDropdown(analogChannelDropdown), "CHANNEL");
        this.setInputsInline(true);
        this.setOutput(true, "Number");
        this.setTooltip('Get the analog input from the specific channel (value returned is between 0 and 1023).');
    }
};

Blockly.Blocks['device_plot_bar_graph'] = {
  init: function() {
    this.setHelpUrl('https://www.microbit.co.uk/functions/plot-bar-graph');
    this.setColour(blockColors.led);
    this.appendDummyInput()
        .appendField("plot bar graph");
    this.appendValueInput("VALUE")
        .setCheck("Number")
        .appendField("of");
    this.appendValueInput("HIGH")
        .setCheck("Number")
        .appendField("up to");
    this.setPreviousStatement(true);
    this.setNextStatement(true);
    this.setTooltip('Displays a bar graph of the value compared to high.');
  }
};

Blockly.Blocks['device_get_joystick_state'] = {
    init: function () {
        this.setColour(blockColors.input);
        this.appendDummyInput()
            .appendField("joystick button");
        this.appendDummyInput()
            .appendField(new Blockly.FieldDropdown(joystickButtonDropdown), "BUTTON");
        this.appendDummyInput()
            .appendField("is pressed");
        this.setInputsInline(true);
        this.setOutput(true, "Boolean");
        this.setTooltip('Check whether a joystick button is pressed right now.');
    }
};

Blockly.Blocks['device_joystick_event'] = {
  init: function() {
    this.setHelpUrl('https://www.microbit.co.uk/functions/on-button-pressed');
    this.setColour(blockColors.input);
    this.appendDummyInput()
        .appendField("on joystick button");
    this.appendDummyInput()
        .appendField(new Blockly.FieldDropdown(joystickButtonDropdown), "BUTTON");
    this.appendDummyInput()
        .appendField("pressed");
    this.appendStatementInput("HANDLER")
        .setAlign(Blockly.ALIGN_RIGHT)
        .appendField("do");
    this.setInputsInline(true);
    this.setTooltip('React to a button press.');
  }
};

Blockly.Blocks['device_say_message'] = {
    init: function () {
        this.setHelpUrl('https://www.microbit.co.uk/functions/show-string');
        this.setColour(blockColors.speech);
        this.appendDummyInput()
            .appendField("Say");
        this.appendValueInput("TEXTTOSAY")
            .setCheck("String")
            .setAlign(Blockly.ALIGN_RIGHT)
            .appendField("string");
        this.setPreviousStatement(true);
        this.setNextStatement(true);
        this.setTooltip('speaks the Text');
        this.setInputsInline(true);
    }
};