using System;
using System.Reflection;

namespace Calidus.lib.Data {
    public class DBColumnAttribute : Attribute {
        public readonly string name;
        public readonly DBColumnType type;
        public readonly bool primaryKey;
        public readonly bool autoincrement;

        public FieldInfo? attachedField;

        public DBColumnAttribute(string name, DBColumnType type, bool primaryKey = false, bool autoincrement = false) {
            this.name = name;
            this.type = type;
            this.primaryKey = primaryKey;
            this.autoincrement = autoincrement;
        }

        public DBColumnAttribute setField(FieldInfo field) {
            attachedField = field;
            return this;
        }
    }

    public enum DBColumnType {
        STRING,
        INTEGER,
        LONG_INTEGER,
        LONG_INTEGER_UNSIGNED
    }
}