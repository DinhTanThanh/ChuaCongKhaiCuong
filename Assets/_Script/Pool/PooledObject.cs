using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PooledObject : MonoBehaviour
{
    [SerializeField] protected string poolKey;
    protected Coroutine autoReturnCoroutine;

    public string PoolKey
    {
        get => this.poolKey;
        set => this.poolKey = value;
    }

    public virtual void ReturnToPool()
    {
        this.StopAutoReturn();
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(this.gameObject);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    public virtual void ReturnToPool(float delay)
    {
        this.StopAutoReturn();
        if (!this.gameObject.activeInHierarchy) return;

        this.autoReturnCoroutine = this.StartCoroutine(this.AutoReturnRoutine(delay));
    }

    protected virtual IEnumerator AutoReturnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        this.ReturnToPool();
    }

    public virtual void StopAutoReturn()
    {
        if (this.autoReturnCoroutine != null)
        {
            this.StopCoroutine(this.autoReturnCoroutine);
            this.autoReturnCoroutine = null;
        }
    }

    protected virtual void OnDisable()
    {
        this.StopAutoReturn();
    }
}
