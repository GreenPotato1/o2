
function ChatMessageModel(model, message)
{
  this.id = message.Id;
  this.timestamp = new Date(message.TimestampUtc);
  this.lines = emotify(escapeHtmlLight(message.Text)).split('\n');

  for (var i = 0; i < this.lines.length; i++)
      this.lines[i] = parseUrls(this.lines[i]);

  this.senderName = '';
  this.senderClass = '';
  this.avatar = '';
  switch (message.Sender)
  {
  case Enums.ChatMessageSender.System:
    this.senderName = '';
    this.senderClass = 'by-system';
    break;
  case Enums.ChatMessageSender.Agent:
    this.senderName = 'Agent ' + message.SenderAgentName;
    this.senderClass = 'by-me';
    this.avatar = model.agentStorage.get(message.SenderAgentId).avatarUrl();
    break;
  case Enums.ChatMessageSender.Visitor:
    this.senderName = 'Visitor';
    this.senderClass = 'by-other';
    break;
  }
}