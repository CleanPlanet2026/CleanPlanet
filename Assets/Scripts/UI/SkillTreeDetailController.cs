using System;
using CleanPlanet.Upgrade;
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
            [SerializeField] private string _upgradeId;
            [SerializeField, Min(0)] private int _initialLevel;
            [SerializeField, Min(1)] private int _maxLevel = 1;
            [SerializeField] private string _state;
            [SerializeField] private string _skillName;
            [SerializeField] private string _branch;
            [SerializeField, TextArea] private string _description;
            [SerializeField] private string _currentEffect;
            [SerializeField] private string _nextEffect;
            [SerializeField] private string _cost;

            public Button Button => _button;
            public string UpgradeId => _upgradeId;
            public int InitialLevel => _initialLevel;
            public int MaxLevel => _maxLevel;
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
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private SkillOption[] _options;
        [SerializeField, Min(0)] private int _defaultOptionIndex;

        private readonly UpgradeRuntimeState _runtimeState = new();
        private UnityAction[] _selectionActions;
        private int _selectedOptionIndex;

        private void OnEnable()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogError($"{nameof(SkillTreeDetailController)} requires all text and option references.", this);
                enabled = false;
                return;
            }

            _upgradeButton.onClick.AddListener(UpgradeSelectedOption);
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
            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveListener(UpgradeSelectedOption);
            }

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
            _selectedOptionIndex = optionIndex;
            SkillOption option = _options[optionIndex];
            _skillNameText.text = option.SkillName;
            _branchText.text = option.Branch;
            _descriptionText.text = option.Description;
            RefreshUpgradeState(option);
            option.Button.Select();
        }

        private void UpgradeSelectedOption()
        {
            SkillOption option = _options[_selectedOptionIndex];
            if (_runtimeState.TryUpgrade(option.UpgradeId, option.InitialLevel, option.MaxLevel))
            {
                RefreshUpgradeState(option);
            }
        }

        private void RefreshUpgradeState(SkillOption option)
        {
            int level = _runtimeState.GetLevel(option.UpgradeId, option.InitialLevel);
            bool isMaxLevel = level >= option.MaxLevel;

            string state = isMaxLevel && option.InitialLevel < option.MaxLevel
                ? "● 강화 완료"
                : option.State;
            _stateText.text = $"{state}  Lv. {level}/{option.MaxLevel}";
            _currentEffectText.text = level > option.InitialLevel
                ? option.NextEffect
                : option.CurrentEffect;
            _nextEffectText.text = isMaxLevel ? "최대 레벨" : option.NextEffect;
            _costText.text = isMaxLevel ? "-" : option.Cost;
            _upgradeButton.interactable = !isMaxLevel;
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
                _upgradeButton == null ||
                _options == null ||
                _options.Length == 0)
            {
                return false;
            }

            foreach (SkillOption option in _options)
            {
                if (option == null ||
                    option.Button == null ||
                    string.IsNullOrWhiteSpace(option.UpgradeId) ||
                    option.InitialLevel > option.MaxLevel)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
