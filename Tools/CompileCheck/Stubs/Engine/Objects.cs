// Compile-check stubs: objects, components and a working Transform hierarchy (no scale shear).
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public virtual string name { get; set; } = "";
        public HideFlags hideFlags { get; set; }

        public static T Instantiate<T>(T original) where T : Object => throw new NotSupportedException("Instantiate");
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => throw new NotSupportedException("Instantiate");
        public static T Instantiate<T>(T original, Transform parent) where T : Object => throw new NotSupportedException("Instantiate");
        public static void Destroy(Object obj) { }
        public static void Destroy(Object obj, float t) { }
        public static void DestroyImmediate(Object obj) { }
        public static void DontDestroyOnLoad(Object target) { }
        // Unity 6.5+: the FindObjectsSortMode overloads and FindFirstObjectByType are obsolete (compile errors),
        // so they are left out here to catch any use.
        public static T[] FindObjectsByType<T>() where T : Object => new T[0];
        public static T[] FindObjectsByType<T>(FindObjectsInactive findObjectsInactive) where T : Object => new T[0];
        public static T FindAnyObjectByType<T>() where T : Object => null;
        public int GetInstanceID() => GetHashCode();

        public static implicit operator bool(Object exists) => !ReferenceEquals(exists, null);
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object other) => ReferenceEquals(this, other);
        public override int GetHashCode() => base.GetHashCode();
    }

    [Flags]
    public enum HideFlags { None = 0, HideInHierarchy = 1, HideInInspector = 2, DontSave = 52, HideAndDontSave = 61 }

    public enum FindObjectsInactive { Exclude, Include }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }

    public class GameObject : Object
    {
        private readonly List<Component> components = new List<Component>();

        public GameObject() : this("New Game Object") { }

        public GameObject(string name)
        {
            this.name = name;
            transform = new Transform();
            transform.AttachTo(this);
            components.Add(transform);
        }

        public Transform transform { get; }
        public int layer { get; set; }
        public string tag { get; set; } = "Untagged";
        public bool activeSelf { get; private set; } = true;
        public bool activeInHierarchy => activeSelf;
        public void SetActive(bool value) => activeSelf = value;
        public GameObject gameObject => this;

        public T AddComponent<T>() where T : Component
        {
            var component = (T)Activator.CreateInstance(typeof(T), true);
            component.AttachTo(this);
            components.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : class
        {
            foreach (var c in components)
            {
                if (c is T match) return match;
            }

            return null;
        }

        public bool TryGetComponent<T>(out T component) where T : class
        {
            component = GetComponent<T>();
            return component != null;
        }

        public T GetComponentInParent<T>() where T : class
        {
            for (var t = transform; t != null; t = t.parent)
            {
                var found = t.gameObject.GetComponent<T>();
                if (found != null) return found;
            }

            return null;
        }

        public T GetComponentInChildren<T>(bool includeInactive = false) where T : class
        {
            var found = GetComponent<T>();
            if (found != null) return found;
            for (int i = 0; i < transform.childCount; i++)
            {
                found = transform.GetChild(i).gameObject.GetComponentInChildren<T>(includeInactive);
                if (found != null) return found;
            }

            return null;
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive = false) where T : class
        {
            var list = new List<T>();
            Collect(this, list);
            return list.ToArray();
        }

        private static void Collect<T>(GameObject go, List<T> list) where T : class
        {
            foreach (var c in go.components)
            {
                if (c is T match) list.Add(match);
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                Collect(go.transform.GetChild(i).gameObject, list);
            }
        }

        public static GameObject Find(string name) => null;

        public static GameObject CreatePrimitive(PrimitiveType type)
        {
            var go = new GameObject(type.ToString());
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            switch (type)
            {
                case PrimitiveType.Sphere: go.AddComponent<SphereCollider>(); break;
                case PrimitiveType.Capsule:
                case PrimitiveType.Cylinder: go.AddComponent<CapsuleCollider>(); break;
                default: go.AddComponent<BoxCollider>(); break;
            }

            return go;
        }
    }

    public class Component : Object
    {
        private GameObject owner;

        internal void AttachTo(GameObject gameObject) => owner = gameObject;

        public GameObject gameObject => owner;
        public Transform transform => owner.transform;
        public string tag { get => owner.tag; set => owner.tag = value; }

        public override string name
        {
            get => owner != null ? owner.name : "";
            set { if (owner != null) owner.name = value; }
        }

        public T GetComponent<T>() where T : class => owner.GetComponent<T>();
        public bool TryGetComponent<T>(out T component) where T : class => owner.TryGetComponent(out component);
        public T GetComponentInParent<T>() where T : class => owner.GetComponentInParent<T>();
        public T GetComponentInChildren<T>(bool includeInactive = false) where T : class => owner.GetComponentInChildren<T>(includeInactive);
        public T[] GetComponentsInChildren<T>(bool includeInactive = false) where T : class => owner.GetComponentsInChildren<T>(includeInactive);
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; } = true;
        public bool isActiveAndEnabled => enabled;
    }

    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine) => new Coroutine();
        public void StopCoroutine(Coroutine routine) { }
        public void StopAllCoroutines() { }
        public static void print(object message) => Console.WriteLine(message);
    }

    public sealed class Coroutine { }

    public class YieldInstruction { }

    public sealed class WaitForFixedUpdate : YieldInstruction { }

    public sealed class WaitForSeconds : YieldInstruction
    {
        public WaitForSeconds(float seconds) { }
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => (T)Activator.CreateInstance(typeof(T), true);
        public static ScriptableObject CreateInstance(Type type) => (ScriptableObject)Activator.CreateInstance(type, true);
    }

    public class Transform : Component, IEnumerable
    {
        private Transform parentTransform;
        private readonly List<Transform> children = new List<Transform>();

        public Vector3 localPosition { get; set; }
        public Quaternion localRotation { get; set; } = Quaternion.identity;
        public Vector3 localScale { get; set; } = Vector3.one;

        public Transform parent
        {
            get => parentTransform;
            set => SetParent(value, true);
        }

        public Transform root
        {
            get
            {
                var t = this;
                while (t.parentTransform != null) t = t.parentTransform;
                return t;
            }
        }

        public int childCount => children.Count;
        public Transform GetChild(int index) => children[index];
        public IEnumerator GetEnumerator() => children.GetEnumerator();

        public Vector3 lossyScale => parentTransform == null ? localScale : Vector3.Scale(parentTransform.lossyScale, localScale);

        public Vector3 position
        {
            get => parentTransform == null ? localPosition : parentTransform.TransformPoint(localPosition);
            set => localPosition = parentTransform == null ? value : parentTransform.InverseTransformPoint(value);
        }

        public Quaternion rotation
        {
            get => parentTransform == null ? localRotation : parentTransform.rotation * localRotation;
            set => localRotation = parentTransform == null ? value : Quaternion.Inverse(parentTransform.rotation) * value;
        }

        public Vector3 right => rotation * Vector3.right;
        public Vector3 up => rotation * Vector3.up;
        public Vector3 forward => rotation * Vector3.forward;

        public void SetParent(Transform newParent) => SetParent(newParent, true);

        public void SetParent(Transform newParent, bool worldPositionStays)
        {
            Vector3 worldPosition = position;
            Quaternion worldRotation = rotation;
            parentTransform?.children.Remove(this);
            parentTransform = newParent;
            newParent?.children.Add(this);
            if (worldPositionStays)
            {
                position = worldPosition;
                rotation = worldRotation;
            }
        }

        public void SetPositionAndRotation(Vector3 worldPosition, Quaternion worldRotation)
        {
            position = worldPosition;
            rotation = worldRotation;
        }

        public Vector3 TransformPoint(Vector3 point) => position + rotation * Vector3.Scale(lossyScale, point);

        public Vector3 InverseTransformPoint(Vector3 point)
        {
            Vector3 local = Quaternion.Inverse(rotation) * (point - position);
            Vector3 s = lossyScale;
            return new Vector3(local.x / s.x, local.y / s.y, local.z / s.z);
        }

        public Vector3 TransformDirection(Vector3 direction) => rotation * direction;
        public Vector3 InverseTransformDirection(Vector3 direction) => Quaternion.Inverse(rotation) * direction;
        public Transform Find(string childName) => children.Find(c => c.name == childName);
    }
}
