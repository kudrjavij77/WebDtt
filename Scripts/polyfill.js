var ua = window.navigator.userAgent;
var msie = ua.indexOf("MSIE ");
if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
    /*
     * disable navbar (assuming we-re came from AisExam7.2, ie11)
     */

/*
 *  STRING.ENDSWITH
 */
    String.prototype.endsWith = function(pattern) {
        var d = this.length - pattern.length;
        return d >= 0 && this.lastIndexOf(pattern) === d;
    };
/*
 * ARRAY.FIND
 */
    if (!Array.prototype.find) {
        Object.defineProperty(Array.prototype,
            'find',
            {
                value: function(predicate) {
                    // 1. Let O be ? ToObject(this value).
                    if (this == null) {
                        throw new TypeError('"this" is null or not defined');
                    }

                    var o = Object(this);

                    // 2. Let len be ? ToLength(? Get(O, "length")).
                    var len = o.length >>> 0;

                    // 3. If IsCallable(predicate) is false, throw a TypeError exception.
                    if (typeof predicate !== 'function') {
                        throw new TypeError('predicate must be a function');
                    }

                    // 4. If thisArg was supplied, let T be thisArg; else let T be undefined.
                    var thisArg = arguments[1];

                    // 5. Let k be 0.
                    var k = 0;

                    // 6. Repeat, while k < len
                    while (k < len) {
                        // a. Let Pk be ! ToString(k).
                        // b. Let kValue be ? Get(O, Pk).
                        // c. Let testResult be ToBoolean(? Call(predicate, T, « kValue, k, O »)).
                        // d. If testResult is true, return kValue.
                        var kValue = o[k];
                        if (predicate.call(thisArg, kValue, k, o)) {
                            return kValue;
                        }
                        // e. Increase k by 1.
                        k++;
                    }

                    // 7. Return undefined.
                    return undefined;
                }
            });
    }
};

// COMMON NAPOX CLR ADDONS

/*
 * ARRAY.TRYPUSH // pushes elt if it does not exist in array
 */
    if (!Array.prototype.tryPush) {
        Object.defineProperty(Array.prototype,
            'tryPush',
            {
                value: function(val, propertyName) {
                    if (this == null) {
                        throw new TypeError('"this" is null or not defined');
                    }
                    var o = Object(this);
                    if (propertyName) {
                        if (!val[propertyName]) {
                            return;
                        }
                        var found = o.filter(function(value, index, arr) {
                            return value[propertyName] === val[propertyName];
                        });
                        if (!found.length) {
                            o.push(val);
                        }
                    } else {
                        var found = o.filter(function(value, index, arr) {
                            return value === val;
                        });
                        if (!found.length) {
                            o.push(val);
                        }
                    }
                }
            }
        );
    }

Date.prototype.addDays = function(days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}