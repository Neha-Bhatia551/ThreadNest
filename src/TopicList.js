import Accordion from 'react-bootstrap/Accordion';
import TopicThread from './TopicThread';
import topicData from './dummyData.json';
import Button from 'react-bootstrap/Button';
import ButtonGroup from 'react-bootstrap/ButtonGroup';
import './TopicList.css';
import commentData from './commentData.json';
import Badge from 'react-bootstrap/Badge';
import { useEffect, useState } from 'react';
import Form from 'react-bootstrap/Form';
import { FormGroup } from 'react-bootstrap';

function TopicList() {

  function saveTopic(e,data) {
    console.log(e);
    topics.concat({
      "id": commentData.length + 1,
      "title" : e.title,
      "content" : e.content,
      "rating" : 0

    });
    console.log(topics);
  } 

  const [comments, setComments] = useState(commentData);
  const [topics, setTopics] = useState(topicData);

  useEffect(() => {
    setComments(comments);
    setTopics(topics);
  });
  return (
    <div>
      <Form>
        <Form.Group className="mb-3" controlId="exampleForm.ControlInput1">
          <Form.Label>Topic subject </Form.Label>
          <Form.Control type="text" placeholder="topic content" />
        </Form.Group>
        <Form.Group className="mb-3" controlId="exampleForm.ControlTextarea1">
          <Form.Label>Topic content</Form.Label>
          <Form.Control as="textarea" rows={3} />
        </Form.Group>
        <FormGroup>
          <Button variant='primary' type="submit" onClick={saveTopic}>Save changes</Button>

        </FormGroup>
        
      </Form>
      <Accordion defaultActiveKey="0">
        {topics.map((topic, index) => (
          <Accordion.Item eventKey={index.toString()} key={index}>
            <Accordion.Header>{topic.title}
              <Badge bg="primary" pill>
                {comments.filter(comment => comment.title_id === topic.id).length}
              </Badge>
            </Accordion.Header>
            <Accordion.Body>
              {topic.content}
              <TopicThread comments={commentData.filter(comment => comment.title_id === topic.id)}></TopicThread>
            </Accordion.Body>
            <strong className="accordion-footer">  Rating: {topic.rating}
              <ButtonGroup size="sm">
                <Button variant='success'>+</Button>
                <Button variant='danger'>-</Button>
              </ButtonGroup>
            </strong>
          </Accordion.Item>
        ))};
      </Accordion>
    </div>
  );
}

export default TopicList;