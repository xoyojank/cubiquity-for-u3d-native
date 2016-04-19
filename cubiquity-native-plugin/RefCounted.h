#pragma once
#include <intrin.h>

class RefCounted
{
    volatile long refCount;
protected:
    virtual ~RefCounted() {}
public:
    RefCounted()
        : refCount(1)
    {}

    void AddRef();
    void Release();
};


inline void RefCounted::AddRef()
{
    _InterlockedIncrement(&this->refCount);
}

inline void RefCounted::Release()
{
    if (0 == _InterlockedDecrement(&this->refCount))
    {
        delete this;
    }
}
