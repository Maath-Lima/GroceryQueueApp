# RabbitMQ

# AMPQ Protocol

## What is?

**Advanced Message Queue Protocol**

Enables cliente applications to communicate with messaging middlewares brokers

## Brokers and their role

Messaging brokers receive messages from *[publishers](https://www.rabbitmq.com/publishers.html)* (applications that publish them, also known as producers) and route them to *[consumers](https://www.rabbitmq.com/consumers.html)* (applications that process them).

Network protocol ⇒ All applications involved can reside on different machines

## Model in brief

- Messages are published to ******************exchanges (******************post office or mailboxe******************)******************
- Exchanges distribute message copies to queues
    - It uses rules called **bindings**
- The broker either deliver messages to consumers subscribed to queues or consumers fetch/pull messages from queues on demand

********************Message acknowledgments******************** 

When a message is delivered to a consumer the consumer *notifies the broker*, either automatically or as soon as the application developer chooses to do so. When message **acknowledgements are in use**, a broker will only completely remove a message from a queue when it receives a notification for that message

When a message cannot be routed, messages may be *returned* to publishers, dropped, or, if the broker implements an extension, placed into a so-called "dead letter queue"

## Exchanges

Exchanges are entities where messages are sent to

Exchanges take a message and route it into zero or more queues

****************************************************Most importante attributes****************************************************

Name

Durability (Exchanges survive broker restart)

Auto-delete (exchange is deleted when last queue is unbound from it)

Exchanges can be durable or transient. Durable survives broker restart

### ************Default exchange************

Is a direct exchange with no namepre-declared by the broker

Very useful for simple applications because every queue that is created is automatically bound to it with a routing key which is the same as the queue name   

> For example, when you declare a queue with the name of "search-indexing-online", the AMQP 0-9-1 broker will bind it to the default exchange using "search-indexing-online" as the routing key
> 

### ******************************Direct exchange******************************

A direct exchange delivers messages to queues based on the message routing key

A direct exchange is ideal for the unicast routing of messages

They can be used for multicast routing as well

### ****************Fanout exchange****************

Routes messages to all of the queues that are bound to it and the routing key is ignored

A copy of the message is delivered for N bound it queues

Fanout exchanges are ideal for the broadcast routing of messages

### ****************************Topic exchange****************************

Route messages to one or many queues based on matching between a message routing key and the pattern that was used to bind a queue to an exchange

Whenever a problem involves multiple consumers/applications that selectively choose which type of messages they want to receive, the use of topic exchanges should be considered

The binding key must also be in the same form. The logic behind the topic exchange is similar to a direct one - a message sent with a particular routing key will be delivered to all the queues that are bound with a matching binding key. However there are two important special cases for binding keys:

- * (star) can substitute for exactly one word.
- # (hash) can substitute for zero or more words.

> Topic exchange is powerful and can behave like other exchanges. When a queue is bound with "#" (hash) binding key - it will receive all the messages, regardless of the routing key - like in fanout exchange. When special characters "*" (star) and "#" (hash) aren't used in bindings, the topic exchange will behave just like a direct one.
> 

### ********************************Headers exchange********************************

Is designed for routing on multiple attributes that are more easily expressed as message headers than a routing key

It is possible to bind a queue using more than one header for matching:

- x-match argument
    - Any - Just one matching header is sufficient
    - All - All the values must match
- For "any" and "all", headers beginning with the string `x-` will not be used to evaluate matches. Setting `"x-match"` to `"any-with-x"` or `"all-with-x"` will also use headers beginning with the string `x-` to evaluate matches

> To list the exchanges on the server you can run the ever useful `rabbitmqctl`:
**`sudo rabbitmqctl list_exchanges`**
> 

## Queues

They store messages that are consumed by applications

**********************Properties:**********************

- Name
- Durable
- Exclusive (used by only one connection and the queue will be deleted when that connection closes)
- Auto-delete (queue that has had at least one consumer is deleted when last consumer unsubscribes)

Before a queue can be used it has to be declared

Declaring a queue will cause it to be created if it does not already exist. The declaration will have no effect if the queue does already exist and its attributes are the same as those in the declaration

> When the existing queue attributes are not the same as those in the declaration a channel-level exception with code **406 (PRECONDITION_FAILED)** will be raised
> 

### **********************Queue Names**********************

Applications may pick queue names or ask the broker to generate a name for them

Pass a empty string as the queue name argumento to let the broker generate a unique queue name

> Queue names starting with "amq." are reserved for internal use by the broker. Attempts to declare a queue with a name that violates this rule will result in a channel-level exception with reply code 403 (ACCESS_REFUSED).
> 

### Queue Durability

Queues can be declared as durable or transient

Metadata of a durable queue is stored on disk, while metadata of a transient queue is stored in memory when possible

In environments and use cases where durability is important, applications must use durable queues *and* make sure that publish mark published messages as persisted

In the .NET client, when we supply no parameters to `QueueDeclare()` we create a non-durable, exclusive, autodelete queue with a generated name:

```
var queueName = channel.QueueDeclare().QueueName;
```

## Bindings

Bindings are rules that exchanges use to route message to queues

The routing key acts like a filter

If a message cannot be routed to any queue it is either dropped or returned to the publisher, depending on message attributes the publisher has set

You can list existing bindings using, you guessed it,

```
rabbitmqctl list_bindings
```

## Consumers

- Subscribe to have messages delivered to them (`"push API"`): this is the recommended option
- Polling (`"pull API"`): this way is highly inefficient and should be avoided in most cases

With the "push API", applications have to indicate interest in consuming messages from a particular queue

It is possible to have more than one consumer per queue or to register an *exclusive consumer* (excludes all other consumers from the queue while it is consuming).

### Message acknowledgments

**When should the broker remove messages from queues?**

After broker sends a message to an application (**`basic.deliver`** or **`basic.get-ok`**)

After the application sends back an acknowledgment (`basic.ack`)

If a consumer dies without sending an acknowledgement, the broker will redeliver it to another consumer or, if none are available at the time, the broker will wait until at least one consumer is registered for the same queue before attempting redelivery

A timeout (30 minutes by default) is enforced on consumer delivery acknowledgement. This helps detect buggy (stuck) consumers that never acknowledge deliveries. You can increase this timeout as described in [Delivery Acknowledgement Timeout](https://www.rabbitmq.com/consumers.html#acknowledgement-timeout).

Acknowledgement must be sent on the same channel that received the delivery. Attempts to acknowledge using a different channel will result in a channel-level protocol exception.

It's a common mistake to miss the `BasicAck`. It's an easy error, but the consequences are serious. Messages will be redelivered when your client quits (which may look like random redelivery), but RabbitMQ will eat more and more memory as it won't be able to release any unacked messages.

In order to debug this kind of mistake you can use `rabbitmqctl` to print the `messages_unacknowledged` field:

```
sudo rabbitmqctl list_queues name messages_ready messages_unacknowledged
```

### Rejecting a message

An application can indicate to the broker that message processing has failed

When rejecting a message, an application can ask the broker to discard or requeue it

> When there is only one consumer on a queue, make sure you do not create infinite message delivery loops by rejecting and requeueing a message from the same consumer over and over again.
> 

### ****Negative Acknowledgements****

Messages are rejected with the basic.reject method. There is one limitation that `basic.reject` has: there is no way to reject multiple messages as you can do with acknowledgements

RabbitMQ provides an extension known as *negative acknowledgements* or *nacks*

### Prefetching messages

For cases when multiple consumers share a queue, it is useful to be able to specify how many messages each consumer can be sent at once before sending the next acknowledgement. This can be used as a simple load balancing technique or to improve throughput if messages tend to be published in batches

## Messages

### Attributes

- Content type
- Content encoding
- Routing key
- Delivery mode (persistent or not)
- Message priority
- Message publishing timestamp
- Expiration period
- Publisher application id

Some attributes are optional and known as *******headers******* 

Message attributes are set when a message is published

Messages have a *payload* (the data that they carry)

It’s possible for messages to contain only attributes and no payload

Messages may be published as persistent, which makes the broker persist them to disk

> Simply publishing a message to a durable exchange or the fact that the queue(s) it is routed to are durable doesn't make a message persistent: it all depends on persistence mode of the message itself
> 

## Fair Dispatch

RabbitMQ just dispatches a message when the message enters the queue. It doesn't look at the number of unacknowledged messages for a consumer. It just blindly dispatches every n-th message to the n-th consumer.

In order to change this behavior we can use the `BasicQos`method with the `prefetchCount = 1` setting. This tells RabbitMQ not to give more than one message to a worker at a time. Or, in other words, don't dispatch a new message to a worker until it has processed and acknowledged the previous one. Instead, it will dispatch it to the next worker that is not still busy.

> **Note about queue size**
If all the workers are busy, your queue can fill up. You will want to keep an eye on that, and maybe add more workers, or have some other strategy.
> 

## AMQP Methods

Methods are operations (like HTTP methods)

[Implementing an event bus with RabbitMQ for the development or test environment](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/rabbitmq-event-bus-development-test-environment)

## Notes

**Round-robin dispatching**
