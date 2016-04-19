#pragma once

#include "RefCounted.h"
#include "ConcurrentQueue.h"
#include <concurrent_queue.h>


class RenderCommand : public RefCounted
{
public:
    virtual void DoCommand() = 0;
};

class RenderCommandProcessor
{
public:
    static RenderCommandProcessor* Instance();

    void SendCommand(RenderCommand* command);

    void ProcessCommands();

private:
    concurrency::concurrent_queue<RenderCommand*> commandQueue;
};
