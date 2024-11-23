using System;
using UniRx;

public class ObservableList<T>  where T : class
{
    private ReactiveCollection<T> List;
    private CompositeDisposable disposables;
    private Action<T> AddProcess;
    private Action<T> RemoveProcess;

    public  ObservableList(Action<T> onAdd, Action<T> onRemove)
    {
        // 既存の購読を解除
        disposables?.Dispose();
        disposables = new CompositeDisposable();
        //---------------- コンストラクタ ---------------
        List = new ReactiveCollection<T>();
        AddProcess = onAdd;
        RemoveProcess = onRemove;

        // リストの要素が増えた時
        List.ObserveAdd()
            .Subscribe(addedItem =>
            {
                AddProcess?.Invoke(addedItem.Value);
            })
            .AddTo(disposables);

        // リストの要素が減った時
        List.ObserveRemove()
            .Subscribe(removedItem =>
            {
                RemoveProcess?.Invoke(removedItem.Value);
            })
            .AddTo(disposables);
    }

    //--------------- リスト操作 ---------------
    public void Add(T item)
    {
        if (item == null || List.Contains(item)) return;

        List.Add(item);
    }

    public void Remove(T item)
    {
        if (item == null || !List.Contains(item)) return;

        List.Remove(item);
    }

    // 最初の要素を取得
    public T GetFirstItem()
    {
        return List.Count > 0 ? List[0] : null;
    }

    // 最後の要素を取得
    public T GetLastItem()
    {
        return List.Count > 0 ? List[List.Count - 1] : null;
    }
    // 要素があるかどうか確認
    public bool HasItem()
    {
        if(List.Count > 0)
        {
            return true;
        }
        return false;
    }

    // クリーンアップ
    public void Dispose()
    {
        disposables.Dispose(); // すべての購読を解除
        AddProcess = null;
        RemoveProcess = null;
    }
}
