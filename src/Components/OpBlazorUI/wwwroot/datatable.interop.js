// Estado da tabela (StateKey/StateStorage): guarda e lê página, ordenação e filtros.
// O JSON é montado no .NET; aqui só se escolhe o storage.

export function save(key, storage, json) {
    try {
        getStorage(storage).setItem(key, json);
    } catch {
        // quota cheia ou storage indisponível (aba privada): ignora
    }
}

export function load(key, storage) {
    try {
        return getStorage(storage).getItem(key);
    } catch {
        return null;
    }
}

export function clear(key, storage) {
    try {
        getStorage(storage).removeItem(key);
    } catch {
        // ignora
    }
}

function getStorage(storage) {
    return storage === 'local' ? localStorage : sessionStorage;
}
