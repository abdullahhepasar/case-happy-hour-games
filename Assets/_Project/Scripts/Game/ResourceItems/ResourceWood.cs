public class ResourceWood : Resource
{
    public override void Initialize(SceneGenerator sg, int id)
    {
        this.SceneGenerator = sg;
        this.ResourceID = id;

        this.CurrentValue = this.baseValue;

        this.active = true;

        MeshStatus(this.active);

        eventResourceDone.AddListener(AppValueController.Instance.SaveForce);
    }

    public override void ChangeValueResource(int value, bool updateRemote = true)
    {
        if (!this.active) return;

        this.CurrentValue = value;

        if (updateRemote)
            this.SceneGenerator.ChangeValueResource(this, value);

        CheckStatus();
    }

    private void CheckStatus()
    {
        if (this.CurrentValue <= 0 && this.active)
        {
            this.active = false;

            MeshStatus(this.active);

            eventResourceDone?.Invoke();
        }
    }

    private void MeshStatus(bool active)
    {
        this.meshPrefab.SetActive(active);
    }

    private void OnDisable()
    {
        eventResourceDone.RemoveAllListeners();
    }

    public override void Reset()
    {
        this.active = false;

        this.CurrentValue = baseValue;
    }
}
