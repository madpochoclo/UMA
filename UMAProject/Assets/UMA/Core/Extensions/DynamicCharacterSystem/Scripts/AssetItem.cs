﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UMA
{
    [System.Serializable]
    public class AssetItem
#if UNITY_EDITOR
        : System.IEquatable<AssetItem>, System.IComparable<AssetItem>
#endif
    {
        #region Fields
        private System.Type _TheType;
        public string _BaseTypeName;
        public string _Name;
        public Object _SerializedItem;
        public string _Path;
		public string _Guid;
        public bool IsResource;
        public bool IsAssetBundle;
		public bool IsAddressable;
		public bool IsAlwaysLoaded;
		public string AddressableGroup;
		public string AddressableAddress;
        public string AddressableLabels;
		public int ReferenceCount;

		[System.NonSerialized]
		public Object _editorCachedItem;

        #endregion
        #region Properties
        public System.Type _Type
        {
            get
            {
                if (_TheType != null) return _TheType;

				if (!UMAAssetIndexer.TypeFromString.ContainsKey(_BaseTypeName))
				{
					Debug.Log("unable to find type: " + _BaseTypeName);
					if (_BaseTypeName.Contains("SlotData"))
						return typeof(SlotDataAsset);
					if (_BaseTypeName.Contains("OverlayData"))
						return typeof(OverlayDataAsset);
					return typeof(object);
				}

				_TheType = UMAAssetIndexer.TypeFromString[_BaseTypeName];
                return _TheType;
            }
        }

        public AssetItem CreateSerializedItem(bool ForceItemSave)
        {
            if (ForceItemSave)
            {
                // If this flag is set, then we must serialize the item also (this is used when building the executable).
                return new AssetItem(this._Type, this._Name, this._Path, this.Item);
            }
            else
            {
                return new AssetItem(this._Type, this._Name, this._Path, null);
            }
        }

        public Object Item
        {
            get
            {
#if UNITY_EDITOR
                if (_SerializedItem != null) return _SerializedItem;

				if (IsAddressable)  // this check is so we can test addressables in the editor
				{
					if (Application.isPlaying)
						return null;
					if (_editorCachedItem == null)
					{
                        Debug.Log("Getting editor cached item: " + _Name);
						_editorCachedItem = GetItem();
					}
					// _editorCachedItem is never saved.
					return _editorCachedItem;
				}
	 
				CacheSerializedItem(); 
                return _SerializedItem;
#else
                return _SerializedItem;
#endif
            }
        }

        public string _AssetBaseName
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(_Path);
            }
        }

		private Object GetItem()
		{
#if UNITY_EDITOR
			Object itemObject = AssetDatabase.LoadAssetAtPath(_Path, _Type);
			if (itemObject == null)
			{
				// uhoh. It's gone.
				if (!string.IsNullOrEmpty(_Guid))
				{
					// at least we have a guid. Let's try to find it from that...
					_Path = AssetDatabase.GUIDToAssetPath(_Guid);
					if (!string.IsNullOrEmpty(_Path))
					{
						itemObject = AssetDatabase.LoadAssetAtPath(_Path, _Type);
					}
				}
				// No guid, or couldn't even find by GUID.
				// Let's search for it?
				if (itemObject == null)
				{
					string s = _Type.Name;
					string[] guids = AssetDatabase.FindAssets(_Name + " t:" + s);
					if (guids.Length > 0)
					{
						_Guid = guids[0];
						_Path = AssetDatabase.GUIDToAssetPath(_Guid);
						itemObject = AssetDatabase.LoadAssetAtPath(_Path, _Type);
					}
				}
			}
#else
			Object itemObject = null;
#endif
			return itemObject;
		}

		public void CacheSerializedItem()
		{
#if UNITY_EDITOR
			if (_SerializedItem != null) return;
#if SUPER_LOGGING
            Debug.Log("Loading item in AssetItem: " + _Name);
#endif
			//if (IsAddressable) return;

			_SerializedItem = GetItem();
#endif
        }

        public static string GetEvilName(Object o)
        {
            if (!o)
            {
                return "<Not Found!>";
            }
            if (o is SlotDataAsset)
            {
                SlotDataAsset sd = o as SlotDataAsset;
                return sd.slotName;
            }
            if (o is OverlayDataAsset)
            {
                OverlayDataAsset od = o as OverlayDataAsset;
                return od.overlayName;
            }
            if (o is RaceData)
            {
                return (o as RaceData).raceName;
            }

            return o.name;
        }

        public string EvilName
        {
            get
            {
                Object o = Item;
                return GetEvilName(o);
            }
        }
#endregion

        public void AddReference()
        {
            ReferenceCount++;
        }

        public void FreeReference()
        {
            ReferenceCount = 0;
            _SerializedItem = null;
        }

		public void ReleaseItem()
		{
            if (IsAddressable)
            {
                ReferenceCount--;
                if (ReferenceCount < 1)
                {
                    FreeReference();
                }
            }
            else
            {
                FreeReference();
            }
        }

#region Methods (edit time)
#if UNITY_EDITOR

		public string ToString(string SortOrder)
        {
            if (SortOrder == "AssetName")
                return _AssetBaseName;
            if (SortOrder == "FilePath")
                return _Path;
            return _Name;
        }

        public bool Equals(AssetItem other)
        {
            if (other == null)
                return false;

            if (UMAAssetIndexer.SortOrder == "AssetName")
            {
                if (this._AssetBaseName == other._AssetBaseName)
                    return true;
                else
                    return false;
            }

            if (UMAAssetIndexer.SortOrder == "FilePath")
            {
                if (this._Path == other._Path)
                    return true;
                else
                    return false;

            }

            if (this._Name == other._Name)
                return true;

            return false;
        }

        public int CompareTo(AssetItem other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            if (UMAAssetIndexer.SortOrder == "AssetName")
            {
                return (this._AssetBaseName.CompareTo(other._AssetBaseName));
            }

            if (UMAAssetIndexer.SortOrder == "FilePath")
            {
                return this._Path.CompareTo(other._Path);
            }

            return this._Name.CompareTo(other._Name);
        }

#endif
#endregion
#region Constructors
        public AssetItem(System.Type Type, string Name, string Path, Object Item)
        {
            if (Type == null) return;
            _TheType = Type;
            _BaseTypeName = Type.Name;
            _Name = Name;
            _SerializedItem = Item;
            _Path = Path;
#if UNITY_EDITOR
			_Guid = AssetDatabase.AssetPathToGUID(_Path);
#endif
        }
        public AssetItem(System.Type Type, Object Item)
        {
            if (Type == null) return;
#if UNITY_EDITOR
            _Path = AssetDatabase.GetAssetPath(Item.GetInstanceID());
			_Guid = AssetDatabase.AssetPathToGUID(_Path);
#endif
            _TheType = Type;
            _BaseTypeName = Type.Name;
            _SerializedItem = Item;
            _Name = EvilName;
        }
#endregion
    }
}
