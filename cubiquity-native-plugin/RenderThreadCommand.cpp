#include "RenderThreadCommand.h"

RenderCommandProcessor* RenderCommandProcessor::Instance()
{
    static RenderCommandProcessor s_instance;
    return &s_instance;
}

void RenderCommandProcessor::SendCommand(RenderCommand* command)
{
    this->commandQueue.push(command);
}

void RenderCommandProcessor::ProcessCommands()
{
    RenderCommand* command = nullptr;
    while (this->commandQueue.try_pop(command))
    {
        command->DoCommand();
        command->Release();
    }
}

