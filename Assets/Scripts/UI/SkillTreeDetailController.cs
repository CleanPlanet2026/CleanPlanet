using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CleanPlanet.UI
{
    public sealed class SkillTreeDetailController : MonoBehaviour
    {
        [Serializable]
        private sealed class SkillOption
        {
            [SerializeField] private Button _button;
            [SerializeField] private string _state;
            [SerializeField] private string _skillName;
            [SerializeField] private string _branch;
            [SerializeField, TextArea] private string _description;
            [SerializeField] private string _currentEffect;
            [SerializeField] private string _nextEffect;
            [SerializeField] private string _cost;

            public Button Button => _button;
            public string State => _state;
            public string SkillName => _skillName;
            public string Branch => _branch;
            public string Description => _description;
            public string CurrentEffect => _currentEffect;
            public string NextEffect => _nextEffect;
            public string Cost => _cost;
        }

        [SerializeField] private Text _stateText;
        [SerializeField] private Text _skillNameText;
        [SerializeField] private Text _branchText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _currentEffectText;
        [SerializeField] private Text _nextEffectText;
        [SerializeField] private Text _costText;
        [SerializeField] private SkillOption[] _options;
        [SerializeField, Min(0)] private int _defaultOptionIndex;

        private UnityAction[] _selectionActions;

        private void OnEnable()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogError($"{nameof(SkillTreeDetailController)} requires all text and option references.", this);
                enabled = false;
                return;
            }

            _selectionActions = new UnityAction[_options.Length];
            for (int i = 0; i < _options.Length; i++)
            {
                int optionIndex = i;
                _selectionActions[i] = () => SelectOption(optionIndex);
                _options[i].Button.onClick.AddListener(_selectionActions[i]);
            }

            SelectOption(Mathf.Clamp(_defaultOptionIndex, 0, _options.Length - 1));
        }

        private void OnDisable()
        {
            if (_selectionActions == null || _options == null)
            {
                return;
            }

            int count = Mathf.Min(_selectionActions.Length, _options.Length);
            for (int i = 0; i < count; i++)
            {
                if (_options[i]?.Button != null)
                {
                    _options[i].Button.onClick.RemoveListener(_selectionActions[i]);
                }
            }

            _selectionActions = null;
        }

        private void SelectOption(int optionIndex)
        {
            SkillOption option = _options[optionIndex];
            _stateText.text = option.State;
            _skillNameText.text = option.SkillName;
            _branchText.text = option.Branch;
            _descriptionText.text = option.Description;
            _currentEffectText.text = option.CurrentEffect;
            _nextEffectText.text = option.NextEffect;
            _costText.text = option.Cost;
            option.Button.Select();
        }

        private bool HasRequiredReferences()
        {
            if (_stateText == null ||
                _skillNameText == null ||
                _branchText == null ||
                _descriptionText == null ||
                _currentEffectText == null ||
                _nextEffectText == null ||
                _costText == null ||
                _options == null ||
                _options.Length == 0)
            {
                return false;
            }

            foreach (SkillOption option in _options)
            {
                if (option == null || option.Button == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
