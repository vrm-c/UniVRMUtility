using System;
using UnityEngine;
using UnityEngine.UI;
using UniVRM10;

namespace UniVRMUtility.VRMViewer
{
    public class LicensePanel : MonoBehaviour
    {
        [Serializable]
        public struct TextFields
        {
            [SerializeField, Header("Info")]
            Text _textModelTitle;
            [SerializeField]
            Text _textModelVersion;
            [SerializeField]
            Text _textModelAuthor;
            [SerializeField]
            Text _textModelContact;
            [SerializeField]
            Text _textModelReference;
            [SerializeField]
            RawImage _thumbnail;

            [SerializeField, Header("CharacterPermission")]
            Text _textPermissionAllowed;
            [SerializeField]
            Text _textPermissionViolent;
            [SerializeField]
            Text _textPermissionSexual;
            [SerializeField]
            Text _textPermissionCommercial;
            [SerializeField]
            Text _textPermissionOther;

            [SerializeField, Header("DistributionLicense")]
            Text _textDistributionLicense;
            [SerializeField]
            Text _textDistributionOther;

            public void LicenseUpdate(VrmLib.Model context)
            {
#if false
                var meta = context.VRM.extensions.VRM.meta;
                m_textModelTitle.text = meta.title;
                m_textModelVersion.text = meta.version;
                m_textModelAuthor.text = meta.author;
                m_textModelContact.text = meta.contactInformation;
                m_textModelReference.text = meta.reference;

                m_textPermissionAllowed.text = meta.allowedUser.ToString();
                m_textPermissionViolent.text = meta.violentUssage.ToString();
                m_textPermissionSexual.text = meta.sexualUssage.ToString();
                m_textPermissionCommercial.text = meta.commercialUssage.ToString();
                m_textPermissionOther.text = meta.otherPermissionUrl;

                m_textDistributionLicense.text = meta.licenseType.ToString();
                m_textDistributionOther.text = meta.otherLicenseUrl;
#else
                var meta = context.Vrm.Meta;

                _textModelTitle.text = meta.Name;
                _textModelVersion.text = meta.Version;
                _textModelAuthor.text = meta.Author;
                _textModelContact.text = meta.ContactInformation;
                _textModelReference.text = meta.Reference;

                _textPermissionAllowed.text = meta.AvatarPermission.AvatarUsage.ToString();
                _textPermissionViolent.text = meta.AvatarPermission.IsAllowedViolentUsage.ToString();
                _textPermissionSexual.text = meta.AvatarPermission.IsAllowedSexualUsage.ToString();
                _textPermissionCommercial.text = meta.AvatarPermission.CommercialUsage.ToString();
                _textPermissionOther.text = meta.AvatarPermission.OtherPermissionUrl;

                _textDistributionLicense.text = meta.RedistributionLicense.ModificationLicense.ToString();
                _textDistributionOther.text = meta.RedistributionLicense.OtherLicenseUrl;

                if (meta.Thumbnail != null)
                {
                    var thumbnail = new Texture2D(2, 2);
                    thumbnail.LoadImage(meta.Thumbnail.Bytes.ToArray());
                    _thumbnail.texture = thumbnail;
                }
#endif
            }
        }

        [SerializeField]
        private TextFields _texts;

        public void LicenseUpdatefunc(VrmLib.Model context)
        {
            _texts.LicenseUpdate(context);
        }
    }

}
