mergeInto(LibraryManager.library, {
    LarjeAdsgramInit: function (keyPtr, blockIdPtr, debug) {
        var key = UTF8ToString(keyPtr);
        var blockId = UTF8ToString(blockIdPtr);
        if (!window.Adsgram) {
            console.warn("LarjeAdsgram: window.Adsgram is missing, add https://sad.adsgram.ai/js/sad.min.js to the WebGL template");
            return 0;
        }

        window.larjeAdsgramBlocks = window.larjeAdsgramBlocks || {};
        window.larjeAdsgramBlocks[key] = window.Adsgram.init({ blockId: blockId, debug: debug === 1 });
        return 1;
    },

    LarjeAdsgramShow: function (keyPtr, requestId, callback) {
        var key = UTF8ToString(keyPtr);
        var blocks = window.larjeAdsgramBlocks || {};
        var controller = blocks[key];
        var finish = function (status) {
            {{{ makeDynCall('vii', 'callback') }}}(requestId, status);
        };

        if (!controller) {
            finish(2);
            return;
        }

        controller.show()
            .then(function (result) {
                finish(result && result.done ? 1 : 0);
            })
            .catch(function () {
                finish(0);
            });
    }
});
