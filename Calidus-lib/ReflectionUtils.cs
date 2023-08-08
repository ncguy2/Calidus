using System;
using System.Collections.Generic;
using System.Reflection;

// This file plays with some weird behaviours, and thus throws a fair few warnings.
// As long as care is taken when using this class, the warnings are nothing to worry about, so suppress them.

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
// ReSharper disable RedundantAssignment
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.PossibleBoxingAllocation

namespace Calidus {
    public static class ReflectionUtils {
        public static T OverlayObjects<T>(T baseline, T overlay) {
            return (T)OverlayObjectsDirect(baseline, overlay);
        }

        private static object OverlayObjectsDirect(object? baseline, object? overlay) {
            if (baseline == null && overlay == null)
                return null!;
            
            Type type = baseline?.GetType() ?? overlay.GetType();

            if (type.IsPrimitive || type == typeof(string))
                return (overlay ?? baseline)!;
            
            if (type.IsArray) {
                object[] baselineArr = (object[]) baseline;
                object[] overlayArr = (object[]) overlay;

                int len = 0;
                len += baselineArr?.Length ?? 0;
                len += overlayArr?.Length ?? 0;
                
                Array outputArr = Array.CreateInstance(type.GetElementType()!, len);
                int offset = 0;
                if(baselineArr != null) {
                    Array.Copy(baselineArr, 0, outputArr, offset, baselineArr.Length);
                    offset += baselineArr.Length;
                }
                if(overlayArr != null) {
                    Array.Copy(overlayArr, 0, outputArr, offset, overlayArr.Length);
                    offset += overlayArr.Length;
                }
                
                return outputArr;
            }

            if (type == typeof(Dictionary<string, string>)) {
                Dictionary<string, string> dict = new();
                if (baseline != null) {
                    Dictionary<string, string> d = (Dictionary<string, string>) baseline;
                    foreach ((string? key, string? value) in d) 
                        dict[key] = value;
                }
                
                if (overlay != null) {
                    Dictionary<string, string> d = (Dictionary<string, string>) overlay;
                    foreach ((string? key, string? value) in d)
                        dict[key] = value;
                }
                
                return dict;
            }
            
            object output = Activator.CreateInstance(type);
            foreach (FieldInfo field in type.GetFields()) {
                object? baselineChild = field.GetValue(baseline);
                object? overlayChild = field.GetValue(overlay);
                object value = OverlayObjectsDirect(baselineChild, overlayChild);
                field.SetValue(output, value);
            }

            return output;
        }
    }
}