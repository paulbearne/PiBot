'use strict';

goog.require('Blockly.JavaScript');

Blockly.JavaScript['device_forever'] = function(block) {
    // Do while(true) loop.
    var branch = Blockly.JavaScript.statementToCode(block, 'DO');
    branch = Blockly.JavaScript.addLoopTrap(branch, block.id);
    return 'while (true) {\nrunEventsHelper();\n' + branch + '}\n';
};

Blockly.JavaScript['device_pause'] = function(block) {
    // Pause statement.
    var pause = Blockly.JavaScript.valueToCode(block, 'PAUSE', Blockly.JavaScript.ORDER_ASSIGNMENT) || '100';
    return 'pauseHelper(' + pause + ');\n';
};


Blockly.JavaScript['device_digital_write_pin'] = function (block) {
    // Turn a specific a GPIO pin on or off.
    var value = Blockly.JavaScript.valueToCode(block, 'VALUE', Blockly.JavaScript.ORDER_ASSIGNMENT) || '4';
    var pin = Blockly.JavaScript.valueToCode(block, 'PIN', Blockly.JavaScript.ORDER_ASSIGNMENT) || '4';
    return 'gpio.setGPIOPin(' + pin + ", " + value + ');\n';
};

Blockly.JavaScript['device_random'] = function(block) {
    // Return a random number
    var limit = Blockly.JavaScript.valueToCode(block, 'LIMIT', Blockly.JavaScript.ORDER_ASSIGNMENT) || '4';
    var code = 'Math.floor(Math.random() * ((' + limit + ') + 1))';
    return [code, Blockly.JavaScript.ORDER_ATOMIC];
};


Blockly.JavaScript['device_get_running_time'] = function(block) {
    // Find how long it has been since the program started
    var code = '(basic.runningTime() | 0)';
    return [code, Blockly.JavaScript.ORDER_ATOMIC];
};


Blockly.JavaScript['device_analog_read_channel'] = function(block) {
    // Get the analog input from the specific channel (value returned is between 0 and 1023)
    var channel = String(Number(block.getFieldValue('CHANNEL')));
    var code = 'adc.getValueFromChannel(' + channel + ')';
    return [code, Blockly.JavaScript.ORDER_ATOMIC];
};


Blockly.JavaScript['device_say_message'] = function (block) {
    var msg = Blockly.JavaScript.valueToCode(block, 'TEXTTOSAY', Blockly.JavaScript.ORDER_ASSIGNMENT) || 'Hello';
    return 'talk.say(' + msg + ');\n';
};
