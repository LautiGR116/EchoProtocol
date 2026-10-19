mergeInto(LibraryManager.library, {
  $EchoWebAudio__deps: ['$WEBAudio', '$jsAudioCreateChannel'],
  $EchoWebAudio: {
    started: false,
    input: null,
    limiter: null,
    values: { ceiling: 0.65, mono: 0, fade: 1 },
    route: function(channel) {
      if (!channel.gain || channel.echoRouted) return;
      channel.echoRouted = true;
      var gain = channel.gain;
      var connect = gain.connect;
      // Unity 6000.3.21f1 connects each channel directly to the destination.
      // Redirect only these Unity-owned nodes, including future reconnects.
      gain.connect = function(destination) {
        var args = Array.prototype.slice.call(arguments);
        if (destination === WEBAudio.audioContext.destination) args[0] = EchoWebAudio.input;
        return connect.apply(this, args);
      };
      if (channel.source) channel.configurePanningNodes();
    },
    start: function() {
      if (EchoWebAudio.started || !WEBAudio.audioContext) return;
      EchoWebAudio.started = true;
      var context = WEBAudio.audioContext;
      var input = EchoWebAudio.input = context.createGain();
      input.gain.value = 0;
      var factory = jsAudioCreateChannel;
      jsAudioCreateChannel = function(callback, userData) {
        var channel = factory(callback, userData);
        EchoWebAudio.route(channel);
        return channel;
      };
      Object.keys(WEBAudio.audioInstances).forEach(function(key) {
        EchoWebAudio.route(WEBAudio.audioInstances[key]);
      });
      if (!context.audioWorklet) {
        window.echoAudioFailed();
        return;
      }
      context.audioWorklet.addModule(new URL('echo-limiter.js', document.baseURI).href).then(function() {
        var node = EchoWebAudio.limiter = new AudioWorkletNode(context, 'echo-limiter', {
          numberOfInputs: 1, numberOfOutputs: 1, outputChannelCount: [2],
          channelCount: 2, channelCountMode: 'explicit', parameterData: EchoWebAudio.values
        });
        node.onprocessorerror = function() {
          input.gain.value = 0;
          window.echoAudioFailed();
        };
        input.connect(node);
        node.connect(context.destination);
        input.gain.value = 1;
        console.info('Echo web audio: master limiter, mono and reduced dynamics ready.');
        window.echoAudioReady(context);
      }).catch(function(error) {
        console.error('Echo web audio initialization failed:', error);
        window.echoAudioFailed();
      });
      Module.deinitializers.push(function() {
        input.disconnect();
        if (EchoWebAudio.limiter) {
          EchoWebAudio.limiter.disconnect();
          EchoWebAudio.limiter.port.close();
        }
        jsAudioCreateChannel = factory;
      });
    }
  },
  EchoWebAudioUpdate__deps: ['$EchoWebAudio'],
  EchoWebAudioUpdate: function(ceiling, mono, fade) {
    var values = { ceiling: ceiling, mono: mono, fade: fade };
    Object.keys(values).forEach(function(key) {
      if (EchoWebAudio.limiter && EchoWebAudio.values[key] !== values[key])
        EchoWebAudio.limiter.parameters.get(key).value = values[key];
      EchoWebAudio.values[key] = values[key];
    });
    EchoWebAudio.start();
  },
  EchoWebExit: function() { window.echoExit(); },
  EchoWebPointerState: function() {
    if (window.echoWebCaptureFailed) { window.echoWebCaptureFailed = false; return -1; }
    if (document.pointerLockElement !== Module.canvas) return 0;
    if (window.echoWebResumeAfterCapture) { window.echoWebResumeAfterCapture = false; return 2; }
    return 1;
  }
});
